# Test PowerShell script to verify XML logic matches WinToolkit
$testXml = @"
<WIM>
<TOTALBYTES>1000000</TOTALBYTES>
<IMAGE INDEX="1">
<DIRCOUNT>100</DIRCOUNT>
<FILECOUNT>200</FILECOUNT>
<TOTALBYTES>500000</TOTALBYTES>
<HARDLINKBYTES>0</HARDLINKBYTES>
<CREATIONTIME>
<HIGHPART>0x01DA0000</HIGHPART>
<LOWPART>0x12345678</LOWPART>
</CREATIONTIME>
<LASTMODIFICATIONTIME>
<HIGHPART>0x01DA0000</HIGHPART>
<LOWPART>0x87654321</LOWPART>
</LASTMODIFICATIONTIME>
<WIMBOOT>0</WIMBOOT>
<NAME>Windows 10 Pro</NAME>
<DESCRIPTION>Windows 10 Professional</DESCRIPTION>
</IMAGE>
<IMAGE INDEX="2">
<DIRCOUNT>150</DIRCOUNT>
<FILECOUNT>300</FILECOUNT>
<TOTALBYTES>600000</TOTALBYTES>
<HARDLINKBYTES>0</HARDLINKBYTES>
<CREATIONTIME>
<HIGHPART>0x01DA0000</HIGHPART>
<LOWPART>0x11111111</LOWPART>
</CREATIONTIME>
<LASTMODIFICATIONTIME>
<HIGHPART>0x01DA0000</HIGHPART>
<LOWPART>0x22222222</LOWPART>
</LASTMODIFICATIONTIME>
<WIMBOOT>0</WIMBOOT>
<NAME>Windows 10 Enterprise</NAME>
<DESCRIPTION>Windows 10 Enterprise Edition</DESCRIPTION>
</IMAGE>
</WIM>
"@

Write-Host "Original XML:"
Write-Host $testXml
Write-Host "`n" + "="*50 + "`n"

# Simulate WinToolkit's Regex.Split logic
$parts = $testXml -split "<IMAGE INDEX="
Write-Host "Split result ($($parts.Count) parts):"
for ($i = 0; $i -lt $parts.Count; $i++) {
    Write-Host "Part ${i}:"
    if ($i -gt 0) {
        $part = "<IMAGE INDEX=" + $parts[$i]
        Write-Host "  [Reconstructed with prefix]: $($part.Substring(0, [Math]::Min(100, $part.Length)))..."
    } else {
        Write-Host "  [Original]: $($parts[$i].Substring(0, [Math]::Min(100, $parts[$i].Length)))..."
    }
    Write-Host ""
}
