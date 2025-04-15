# TEMPLATE: Replace PageName with your actual page name (e.g., UserSettings)
function Initialize-PageNamePage {
    process {
        Write-BucketLog -Data "Initializing PageNamePage" -Level "Info"

        #region Data Initialization
        # TEMPLATE: Replace pageNameLower with your actual page name in lowercase (e.g., userSettings)
        # Define script-level variables for global access across event handlers
        $script:pageNameLower_SampleProperty = "Sample Value"
        
        # Initialize file paths if needed
        $script:pageNameLower_SamplePath = Join-Path -Path $script:workingDirectory -ChildPath 'Sample'
        
        # Example: Calculate statistics for display
        try {
            $script:pageNameLower_SampleCount = 0
            # Add your data initialization logic here
        }
        catch {
            Write-BucketLog -Data "Error initializing PageNamePage data: $_" -Level "Error"
            $script:pageNameLower_SampleCount = "N/A"
        }
        #endregion Data Initialization

        #region Event Handlers
        # Handler for when the page is loaded
        $pageLoadedHandler = {
            param($senderObj, $e)

            Write-BucketLog -Data "PageNamePage loaded, setting up handlers" -Level "Info"

            #region UI Element References
            # Get the page itself
            $page = $senderObj

            # Get references to UI elements
            # TEMPLATE: Replace PageName with your actual page name
            $sampleButton = $page.FindName("PageName_SampleButton")
            $cancelButton = $page.FindName("PageName_CancelButton")
            $previousButton = $page.FindName("PageName_PreviousButton")
            $nextButton = $page.FindName("PageName_NextButton")
            #endregion UI Element References

            #region Button Event Handlers
            # Set up Sample button event
            if ($sampleButton) {
                $sampleButton.Add_Click({
                    param($senderObj, $e)

                    Write-BucketLog -Data "Sample button clicked in PageNamePage" -Level "Info"
                    # Add your button click logic here
                })
            }

            # Set up Cancel button event
            if ($cancelButton) {
                $cancelButton.Add_Click({
                    param($senderObj, $e)

                    Write-BucketLog -Data "Cancel button clicked in PageNamePage" -Level "Info"
                    Invoke-BucketHomePage
                })
            }

            # Set up Previous button event
            if ($previousButton) {
                $previousButton.Add_Click({
                    param($senderObj, $e)

                    Write-BucketLog -Data "Previous button clicked in PageNamePage" -Level "Info"
                    # Navigate to previous page
                    # Example: Invoke-BucketPreviousPage
                })
            }

            # Set up Next button event
            if ($nextButton) {
                $nextButton.Add_Click({
                    param($senderObj, $e)

                    Write-BucketLog -Data "Next button clicked in PageNamePage" -Level "Info"
                    # Navigate to next page
                    # Example: Invoke-BucketNextPage
                })
            }
            #endregion Button Event Handlers
        }
        #endregion Event Handlers

        #region Page Navigation
        # Use the navigation service to navigate to the ${PageName}Page
        $page = Invoke-BucketNavigationService -PageName "PageNamePage"
        
        # Add the loaded event handler
        $page.Add_Loaded($pageLoadedHandler)

        #region Data Context Creation
        # Create a data context object with all the properties needed by the UI
        $dataContext = [PSCustomObject]@{
            # System information
            # TEMPLATE: Replace pageNameLower with your actual page name in lowercase
            SampleProperty = $script:pageNameLower_SampleProperty
            SampleCount    = $script:pageNameLower_SampleCount
            # Add more properties as needed
        }
        
        # Set the data context for the page
        $page.DataContext = $dataContext
        #endregion Data Context Creation
        #endregion Page Navigation
    }
}
