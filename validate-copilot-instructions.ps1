# Copilot Instructions - Installation Validator
# Run this script to verify all instruction files are properly installed

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  COPILOT INSTRUCTIONS - VALIDATOR" -ForegroundColor White
Write-Host "========================================`n" -ForegroundColor Cyan

$filesExpected = @(
    ".github\copilot-instructions.md",
    ".github\copilot-instructions-template.md",
    "Assets\Scripts\Components\.copilot-instructions.md",
    "Assets\Scripts\Systems\.copilot-instructions.md",
    "Assets\Scripts\Aspects\.copilot-instructions.md",
    "Assets\Scripts\Utilities\.copilot-instructions.md",
    "Assets\Scripts\Converters\.copilot-instructions.md",
    "COPILOT-INSTRUCTIONS-README.md",
    "COPILOT-INSTRUCTIONS-INSTALLATION.md",
    "GET-STARTED-WITH-COPILOT.md",
    "SUMMARY-COPILOT-INSTRUCTIONS.md"
)

$filesFound = 0
$filesMissing = 0
$totalSize = 0

Write-Host "Checking installation...`n" -ForegroundColor Yellow

foreach ($file in $filesExpected) {
    if (Test-Path $file) {
        $fileInfo = Get-Item $file
        $sizeKB = [math]::Round($fileInfo.Length / 1KB, 2)
        $totalSize += $fileInfo.Length
        Write-Host "  ? $file" -ForegroundColor Green -NoNewline
        Write-Host " ($sizeKB KB)" -ForegroundColor Gray
        $filesFound++
    } else {
        Write-Host "  ? $file" -ForegroundColor Red -NoNewline
        Write-Host " (MISSING)" -ForegroundColor Red
        $filesMissing++
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  RESULTS" -ForegroundColor White
Write-Host "========================================`n" -ForegroundColor Cyan

$totalSizeKB = [math]::Round($totalSize / 1KB, 2)

Write-Host "Files Found:    " -NoNewline -ForegroundColor White
Write-Host "$filesFound / $($filesExpected.Count)" -ForegroundColor $(if ($filesFound -eq $filesExpected.Count) { "Green" } else { "Yellow" })

Write-Host "Files Missing:  " -NoNewline -ForegroundColor White
Write-Host "$filesMissing" -ForegroundColor $(if ($filesMissing -eq 0) { "Green" } else { "Red" })

Write-Host "Total Size:     " -NoNewline -ForegroundColor White
Write-Host "$totalSizeKB KB" -ForegroundColor Green

Write-Host "`n========================================" -ForegroundColor Cyan

if ($filesMissing -eq 0) {
    Write-Host "  ? INSTALLATION COMPLETE!" -ForegroundColor Green
    Write-Host "========================================`n" -ForegroundColor Cyan
    
    Write-Host "Next Steps:" -ForegroundColor Yellow
    Write-Host "  1. Read: .github\copilot-instructions.md" -ForegroundColor White
    Write-Host "  2. Open: GET-STARTED-WITH-COPILOT.md" -ForegroundColor White
    Write-Host "  3. Test: Try a sample prompt in VS Code" -ForegroundColor White
    Write-Host "`n"
} else {
    Write-Host "  ??  INSTALLATION INCOMPLETE" -ForegroundColor Yellow
    Write-Host "========================================`n" -ForegroundColor Cyan
    
    Write-Host "Some files are missing. Please check the installation." -ForegroundColor Red
    Write-Host "`n"
}

# Additional checks
Write-Host "Additional Checks:" -ForegroundColor Yellow
Write-Host ""

# Check for .github folder
if (Test-Path ".github") {
    Write-Host "  ? .github folder exists" -ForegroundColor Green
} else {
    Write-Host "  ? .github folder missing" -ForegroundColor Red
}

# Check for Assets/Scripts folders
$scriptFolders = @("Components", "Systems", "Aspects", "Utilities", "Converters")
foreach ($folder in $scriptFolders) {
    $path = "Assets\Scripts\$folder"
    if (Test-Path $path) {
        Write-Host "  ? $path exists" -ForegroundColor Green
    } else {
        Write-Host "  ? $path missing" -ForegroundColor Red
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host ""

# Summary statistics
Write-Host "Installation Statistics:" -ForegroundColor Yellow
Write-Host "  • Total files: $($filesExpected.Count)" -ForegroundColor White
Write-Host "  • Total size: $totalSizeKB KB" -ForegroundColor White
Write-Host "  • Estimated lines: ~2,700" -ForegroundColor White
Write-Host "  • Code examples: 50+" -ForegroundColor White
Write-Host ""

Write-Host "========================================`n" -ForegroundColor Cyan
