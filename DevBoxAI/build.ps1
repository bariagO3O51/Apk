# DevBoxAI Build Script für Windows
# Kompiliert die DevBoxAI.exe

param(
    [string]$Configuration = "Release",
    [switch]$Clean = $false
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "DevBoxAI Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$SolutionFile = "DevBoxAI.sln"
$OutputDir = ".\build\$Configuration"

# Check if solution exists
if (-not (Test-Path $SolutionFile)) {
    Write-Host "Error: Solution file not found: $SolutionFile" -ForegroundColor Red
    exit 1
}

# Clean if requested
if ($Clean) {
    Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
    if (Test-Path ".\build") {
        Remove-Item -Path ".\build" -Recurse -Force
    }
    dotnet clean $SolutionFile --configuration $Configuration
}

# Restore NuGet packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore $SolutionFile

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: NuGet restore failed" -ForegroundColor Red
    exit 1
}

# Build solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build $SolutionFile --configuration $Configuration --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Build failed" -ForegroundColor Red
    exit 1
}

# Publish for distribution
Write-Host "Publishing application..." -ForegroundColor Yellow
$PublishDir = ".\build\publish"

dotnet publish ".\src\DevBoxAI\DevBoxAI.csproj" `
    --configuration $Configuration `
    --output $PublishDir `
    --self-contained true `
    --runtime win-x64 `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishReadyToRun=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Publish failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Build Successful!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Output: $PublishDir\DevBoxAI.exe" -ForegroundColor Cyan
Write-Host ""

# Display file info
if (Test-Path "$PublishDir\DevBoxAI.exe") {
    $FileInfo = Get-Item "$PublishDir\DevBoxAI.exe"
    $FileSize = [math]::Round($FileInfo.Length / 1MB, 2)
    Write-Host "File Size: $FileSize MB" -ForegroundColor Cyan
    Write-Host "Path: $($FileInfo.FullName)" -ForegroundColor Cyan
}
