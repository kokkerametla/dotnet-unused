#!/bin/bash
# Pack the dotnet tool for NuGet distribution
# Usage: ./scripts/pack.sh [version]

VERSION=${1:-"1.0.0"}

echo -e "\033[0;36mPacking DotnetUnused as dotnet tool (version $VERSION)...\033[0m"

# Update version in csproj if provided
if [ "$VERSION" != "1.0.0" ]; then
    CSPROJ_PATH="DotnetUnused/DotnetUnused.csproj"
    sed -i.bak "s|<Version>.*</Version>|<Version>$VERSION</Version>|g" $CSPROJ_PATH
    sed -i.bak "s|<AssemblyVersion>.*</AssemblyVersion>|<AssemblyVersion>$VERSION.0</AssemblyVersion>|g" $CSPROJ_PATH
    sed -i.bak "s|<FileVersion>.*</FileVersion>|<FileVersion>$VERSION.0</FileVersion>|g" $CSPROJ_PATH
    rm $CSPROJ_PATH.bak
    echo -e "\033[0;32mUpdated version to $VERSION\033[0m"
fi

# Clean previous builds
echo -e "\033[0;33mCleaning previous builds...\033[0m"
dotnet clean DotnetUnused/DotnetUnused.csproj -c Release

# Build
echo -e "\033[0;33mBuilding...\033[0m"
dotnet build DotnetUnused/DotnetUnused.csproj -c Release

if [ $? -ne 0 ]; then
    echo -e "\033[0;31mBuild failed!\033[0m"
    exit 1
fi

# Pack as NuGet package
echo -e "\033[0;33mPacking...\033[0m"
dotnet pack DotnetUnused/DotnetUnused.csproj -c Release --no-build -o ./artifacts

if [ $? -ne 0 ]; then
    echo -e "\033[0;31mPack failed!\033[0m"
    exit 1
fi

echo ""
echo -e "\033[0;32mSuccess! Package created in ./artifacts/\033[0m"
echo ""
echo -e "\033[0;36mTo install locally for testing:\033[0m"
echo -e "  dotnet tool install --global --add-source ./artifacts DotnetUnused"
echo ""
echo -e "\033[0;36mTo publish to NuGet.org:\033[0m"
echo -e "  dotnet nuget push ./artifacts/DotnetUnused.$VERSION.nupkg --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY"
