# Publish standalone executables for all platforms
# Usage: .\scripts\publish-standalone.ps1 [version]

param(
    [string]$Version = "1.0.0"
)

$runtimes = @("win-x64", "linux-x64", "osx-x64", "osx-arm64")

Write-Host "Publishing standalone executables (version $Version)..." -ForegroundColor Cyan
Write-Host ""

foreach ($runtime in $runtimes) {
    Write-Host "Publishing for $runtime..." -ForegroundColor Yellow

    dotnet publish DotnetUnused\DotnetUnused.csproj `
        -c Release `
        -r $runtime `
        --self-contained true `
        /p:PublishSingleFile=true `
        /p:Version=$Version `
        -o ".\publish\$runtime"

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to publish for $runtime" -ForegroundColor Red
        exit 1
    }

    Write-Host "✓ Published to .\publish\$runtime\" -ForegroundColor Green
    Write-Host ""
}

# Create release archives
Write-Host "Creating release archives..." -ForegroundColor Cyan

if (-not (Test-Path ".\releases")) {
    New-Item -ItemType Directory -Path ".\releases" | Out-Null
}

foreach ($runtime in $runtimes) {
    $archiveName = "dotnet-unused-$Version-$runtime.zip"

    if ($runtime.StartsWith("win")) {
        $exeName = "DotnetUnused.exe"
    } else {
        $exeName = "DotnetUnused"
    }

    Compress-Archive `
        -Path ".\publish\$runtime\$exeName" `
        -DestinationPath ".\releases\$archiveName" `
        -Force

    Write-Host "✓ Created $archiveName" -ForegroundColor Green
}

Write-Host ""
Write-Host "All releases created in .\releases\" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Create a GitHub release (tag: v$Version)" -ForegroundColor White
Write-Host "2. Upload the archives from .\releases\" -ForegroundColor White
Write-Host "3. Or use: gh release create v$Version .\releases\*.zip" -ForegroundColor White
