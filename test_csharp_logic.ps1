# Test our C# XML logic to debug the double processing issue
$testXml = @"
<WIM><TOTALBYTES>1000000</TOTALBYTES><IMAGE INDEX="1"><DIRCOUNT>100</DIRCOUNT><NAME>Windows 10 Education</NAME><DESCRIPTION>Windows 10 Education</DESCRIPTION></IMAGE><IMAGE INDEX="2"><DIRCOUNT>150</DIRCOUNT><NAME>Windows 10 Enterprise</NAME></IMAGE></WIM>
"@

Write-Host "=== Testing WinToolkit Regex.Split Logic ==="
Write-Host "Original XML:"
Write-Host $testXml
Write-Host ""

# Simulate exact WinToolkit logic
$parts = [regex]::Split($testXml, "<IMAGE INDEX=")
Write-Host "Split into $($parts.Count) parts:"

$nData = ""
for ($i = 0; $i -lt $parts.Count; $i++) {
    $DE = $parts[$i]
    Write-Host "Part $i original: '$($DE.Substring(0, [Math]::Min(50, $DE.Length)))...'"

    # WinToolkit logic: if starts with quote, add back prefix
    if ($DE.StartsWith('"')) {
        $DE = "<IMAGE INDEX=" + $DE
        Write-Host "  → Reconstructed: '$($DE.Substring(0, [Math]::Min(50, $DE.Length)))...'"
    }

    # Check if this matches index 1
    if ($DE.StartsWith('<IMAGE INDEX="1">')) {
        Write-Host "  → MATCHES INDEX 1 - would process this section"
    }

    $nData += $DE
}

Write-Host ""
Write-Host "Reconstructed XML:"
Write-Host $nData
Write-Host ""
Write-Host "=== Checking for differences ==="
Write-Host "Original  length: $($testXml.Length)"
Write-Host "Reconstructed length: $($nData.Length)"
Write-Host "Match: $($testXml -eq $nData)"
