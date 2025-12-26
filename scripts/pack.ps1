# Pack the dotnet tool for NuGet distribution
# Usage: .\scripts\pack.ps1 [version]

param(
    [string]$Version = "1.0.0"
)

Write-Host "Packing DotnetUnused as dotnet tool (version $Version)..." -ForegroundColor Cyan

# Update version in csproj if provided
if ($Version -ne "1.0.0") {
    $csprojPath = "DotnetUnused\DotnetUnused.csproj"
    $content = Get-Content $csprojPath -Raw
    $content = $content -replace '<Version>.*?</Version>', "<Version>$Version</Version>"
    $content = $content -replace '<AssemblyVersion>.*?</AssemblyVersion>', "<AssemblyVersion>$Version.0</AssemblyVersion>"
    $content = $content -replace '<FileVersion>.*?</FileVersion>', "<FileVersion>$Version.0</FileVersion>"
    Set-Content $csprojPath $content
    Write-Host "Updated version to $Version" -ForegroundColor Green
}

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean DotnetUnused\DotnetUnused.csproj -c Release

# Build
Write-Host "Building..." -ForegroundColor Yellow
dotnet build DotnetUnused\DotnetUnused.csproj -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Pack as NuGet package
Write-Host "Packing..." -ForegroundColor Yellow
dotnet pack DotnetUnused\DotnetUnused.csproj -c Release --no-build -o .\artifacts

if ($LASTEXITCODE -ne 0) {
    Write-Host "Pack failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Success! Package created in .\artifacts\" -ForegroundColor Green
Write-Host ""
Write-Host "To install locally for testing:" -ForegroundColor Cyan
Write-Host "  dotnet tool install --global --add-source .\artifacts DotnetUnused" -ForegroundColor White
Write-Host ""
Write-Host "To publish to NuGet.org:" -ForegroundColor Cyan
Write-Host "  dotnet nuget push .\artifacts\DotnetUnused.$Version.nupkg --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY" -ForegroundColor White
