#!/bin/bash
# DevBoxAI Build Script für Linux/Mac (Cross-Compile für Windows)

set -e

CONFIGURATION="Release"
CLEAN=false

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        --clean)
            CLEAN=true
            shift
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo "========================================"
echo "DevBoxAI Build Script"
echo "========================================"
echo ""

SOLUTION_FILE="DevBoxAI.sln"
OUTPUT_DIR="./build/$CONFIGURATION"

# Check if solution exists
if [ ! -f "$SOLUTION_FILE" ]; then
    echo "Error: Solution file not found: $SOLUTION_FILE"
    exit 1
fi

# Clean if requested
if [ "$CLEAN" = true ]; then
    echo "Cleaning previous builds..."
    rm -rf ./build
    dotnet clean "$SOLUTION_FILE" --configuration "$CONFIGURATION"
fi

# Restore NuGet packages
echo "Restoring NuGet packages..."
dotnet restore "$SOLUTION_FILE"

# Build solution
echo "Building solution..."
dotnet build "$SOLUTION_FILE" --configuration "$CONFIGURATION" --no-restore

# Publish for Windows distribution
echo "Publishing application for Windows..."
PUBLISH_DIR="./build/publish"

dotnet publish "./src/DevBoxAI/DevBoxAI.csproj" \
    --configuration "$CONFIGURATION" \
    --output "$PUBLISH_DIR" \
    --self-contained true \
    --runtime win-x64 \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:PublishReadyToRun=true

echo ""
echo "========================================"
echo "Build Successful!"
echo "========================================"
echo ""
echo "Output: $PUBLISH_DIR/DevBoxAI.exe"
echo ""

# Display file info
if [ -f "$PUBLISH_DIR/DevBoxAI.exe" ]; then
    FILE_SIZE=$(du -h "$PUBLISH_DIR/DevBoxAI.exe" | cut -f1)
    echo "File Size: $FILE_SIZE"
    echo "Path: $(realpath $PUBLISH_DIR/DevBoxAI.exe)"
fi
