<#
.SYNOPSIS
    Executes a PowerShell script block as a background job with progress tracking.

.DESCRIPTION
    This function provides a standardized way to execute long-running operations as PowerShell Core jobs
    while maintaining responsiveness. It handles job lifecycle management, progress monitoring,
    error handling, and cleanup automatically. The function uses a DispatcherTimer to monitor
    job status and execute callbacks without blocking the main thread.

.NOTES
    Name:        Invoke-BucketBackgroundTask.ps1
    Author:      Mickaël CHAVE
    Created:     06/06/2025
    Version:     25.6.6.1
    Repository:  https://github.com/mchave3/Bucket
    License:     MIT License

.LINK
    https://github.com/mchave3/Bucket

.PARAMETER ScriptBlock
    The PowerShell script block to execute as a background job.

.PARAMETER OnSuccess
    Script block to execute when the task completes successfully. Receives the job result as parameter.

.PARAMETER OnError
    Script block to execute when the task fails. Receives the error information as parameter.

.PARAMETER OnProgress
    Optional script block to execute periodically for custom progress updates.

.PARAMETER TaskName
    Optional name for the task, used in logging and UI messages.

.EXAMPLE
    Invoke-BucketBackgroundTask -ScriptBlock { Get-BucketWimIndex -IsoPath $isoPath } -TaskName "Loading ISO indices" -OnSuccess { param($result) $dataGrid.ItemsSource = $result }
    Executes the WIM index extraction in the background and updates the UI when complete.
#>
function Invoke-BucketBackgroundTask {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ScriptBlock]$ScriptBlock,

        [Parameter(Mandatory = $false)]
        [ScriptBlock]$OnSuccess,

        [Parameter(Mandatory = $false)]
        [ScriptBlock]$OnError,

        [Parameter(Mandatory = $false)]
        [ScriptBlock]$OnProgress,

        [Parameter(Mandatory = $false)]
        [string]$TaskName = "Background Task"
    )

    process {
        #region Initialization
        Write-BucketLog -Data "Starting background task: $TaskName" -Level Info

        # Initialize progress tracking variables
        $script:backgroundTaskJob = $null
        $script:backgroundTaskTimer = $null
        $script:backgroundTaskStartTime = Get-Date

        # Store callbacks in script scope for timer access
        $script:backgroundTaskOnSuccess = $OnSuccess
        $script:backgroundTaskOnError = $OnError
        $script:backgroundTaskOnProgress = $OnProgress
        $script:backgroundTaskName = $TaskName
        #endregion Initialization

        #region Job Execution
        try {
            # Determine the Bucket module path for importing in the Job
            $bucketModulePath = $null
            $currentModule = Get-Module -Name "Bucket"
            if ($currentModule) {
                $bucketModulePath = $currentModule.Path
                Write-BucketLog -Data "Bucket module path: $bucketModulePath" -Level Debug
            }

            # Create a wrapper script block that imports the Bucket module and then executes the user script
            $wrappedScriptBlock = [ScriptBlock]::Create(@"
                # Import the Bucket module in the Job runspace
                if ('$bucketModulePath' -and (Test-Path '$bucketModulePath')) {
                    Import-Module '$bucketModulePath' -Force
                }

                # Execute the original script block
                & {$($ScriptBlock.ToString())}
"@)

            # Start the PowerShell Core job with the wrapped script block
            $script:backgroundTaskJob = Start-Job -ScriptBlock $wrappedScriptBlock
            Write-BucketLog -Data "Background job started with ID: $($script:backgroundTaskJob.Id)" -Level Debug

            # Create DispatcherTimer to monitor job progress
            $script:backgroundTaskTimer = New-Object System.Windows.Threading.DispatcherTimer
            $script:backgroundTaskTimer.Interval = [TimeSpan]::FromMilliseconds(500) # Check every 500ms

            # Timer tick event handler
            $script:backgroundTaskTimer.Add_Tick({
                    try {
                        $job = $script:backgroundTaskJob
                        if (-not $job) {
                            $script:backgroundTaskTimer.Stop()
                            return
                        }

                        # Calculate elapsed time
                        $elapsed = (Get-Date) - $script:backgroundTaskStartTime

                        # Execute custom progress callback if provided
                        if ($script:backgroundTaskOnProgress) {
                            try {
                                & $script:backgroundTaskOnProgress $job $elapsed
                            }
                            catch {
                                Write-BucketLog -Data "Error in OnProgress callback: $_" -Level Warning
                            }
                        }

                        # Check job state
                        Write-BucketLog -Data "Checking job state: $($job.State)" -Level Debug
                        switch ($job.State) {
                            'Completed' {
                                Write-BucketLog -Data "Background task completed successfully" -Level Info
                                $script:backgroundTaskTimer.Stop()

                                # Get job results
                                $result = Receive-Job -Job $job
                                Write-BucketLog -Data "Received job result, type: $($result.GetType().Name), count: $(if($result -is [array]) { $result.Count } else { 1 })" -Level Debug
                                Remove-Job -Job $job
                                $script:backgroundTaskJob = $null

                                # Execute success callback
                                Write-BucketLog -Data "About to execute OnSuccess callback, OnSuccess is null: $($null -eq $script:backgroundTaskOnSuccess)" -Level Debug
                                if ($script:backgroundTaskOnSuccess) {
                                    try {
                                        Write-BucketLog -Data "Executing OnSuccess callback with result" -Level Debug
                                        & $script:backgroundTaskOnSuccess $result
                                        Write-BucketLog -Data "OnSuccess callback completed" -Level Debug
                                    }
                                    catch {
                                        Write-BucketLog -Data "Error in OnSuccess callback: $_" -Level Error
                                    }
                                }
                            }
                            'Failed' {
                                Write-BucketLog -Data "Background task failed" -Level Error
                                $script:backgroundTaskTimer.Stop()

                                # Get error information
                                $errorInfo = $job.ChildJobs[0].JobStateInfo.Reason
                                $jobErrors = Receive-Job -Job $job 2>&1
                                Remove-Job -Job $job
                                $script:backgroundTaskJob = $null

                                # Log detailed error
                                Write-BucketLog -Data "Job error details: $errorInfo" -Level Error
                                if ($jobErrors) {
                                    Write-BucketLog -Data "Job stderr: $jobErrors" -Level Error
                                }

                                # Execute error callback
                                if ($script:backgroundTaskOnError) {
                                    try {
                                        & $script:backgroundTaskOnError @{
                                            ErrorInfo = $errorInfo
                                            JobErrors = $jobErrors
                                            TaskName  = $script:backgroundTaskName
                                        }
                                    }
                                    catch {
                                        Write-BucketLog -Data "Error in OnError callback: $_" -Level Error
                                    }
                                }
                            }
                            'Stopped' {
                                Write-BucketLog -Data "Background task was stopped" -Level Warning
                                $script:backgroundTaskTimer.Stop()
                                Remove-Job -Job $job -Force
                                $script:backgroundTaskJob = $null
                            }
                            # 'Running' - continue monitoring
                        }
                    }
                    catch {
                        Write-BucketLog -Data "Error in background task timer: $_" -Level Error
                        $script:backgroundTaskTimer.Stop()

                        # Cleanup
                        if ($script:backgroundTaskJob) {
                            Remove-Job -Job $script:backgroundTaskJob -Force
                            $script:backgroundTaskJob = $null
                        }
                    }
                })

            # Start monitoring
            $script:backgroundTaskTimer.Start()
            Write-BucketLog -Data "Background task monitoring started" -Level Debug

        }
        catch {
            Write-BucketLog -Data "Failed to start background task: $_" -Level Error

            # Execute error callback
            if ($script:backgroundTaskOnError) {
                try {
                    & $script:backgroundTaskOnError @{
                        ErrorInfo = $_
                        JobErrors = $null
                        TaskName  = $script:backgroundTaskName
                    }
                }
                catch {
                    Write-BucketLog -Data "Error in OnError callback: $_" -Level Error
                }
            }

            throw $_
        }
        #endregion Job Execution
    }
}
