#!/bin/bash

set -euo pipefail

# Required project-specific environment variables:
#    BUILD_DIR="../build"
#    CSPROJ_PATH="./src/"
#    PROJECT_NAME="valid.utilities.csproj"
#    TEST_PROJECT="./test/valid.utilities.test.csproj"
#    NUGET_SERVER="https://www.myget.org/F/valid-nuget-feed/api/v2/package"
#    NUGET_API_KEY=xxx
#    NUGET_SYMBOLS_SERVER="https://www.myget.org/F/valid-nuget-feed/symbols/api/v2/package"
#
# Required, global environment variables:
#    GIT_BRANCH="origin/develop"
#
# Required executables:
#    dotnet
sed -e "s/NUGET_API_KEY/$NUGET_API_KEY/g" nuget.config.in > nuget.config
project=$(basename -s .csproj "$CSPROJ_PATH$PROJECT_NAME")
version=$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$CSPROJ_PATH$PROJECT_NAME")
version_suffix='--version-suffix "pre"'
nupkg_file=./build/$project.$version-pre.nupkg

echo "Project: $project"
echo "Branch: $GIT_BRANCH"
echo "Testing $project version: $version"

dotnet test "$TEST_PROJECT"

# Nuget packages default to "pre" release unless on master
if [ "$GIT_BRANCH" == "master" ]; then
    echo "Building production release"
    nupkg_file=./build/$project.$version.nupkg
    version_suffix=''
fi

dotnet pack "$CSPROJ_PATH$PROJECT_NAME" -o "$BUILD_DIR" --include-symbols $version_suffix

# Only publish when building on master or develop
if [ "$GIT_BRANCH" == "master" ] || [ "$GIT_BRANCH" == "develop" ]; then

    echo "Publishing $nupkg_file to $NUGET_SERVER"

    # Publish to nuget using NUGET_SERVER and NUGET_API_KEY env variables
    dotnet nuget push "$nupkg_file" -s "$NUGET_SERVER" -k "$NUGET_API_KEY" -ss "$NUGET_SYMBOLS_SERVER" -t 60 -n --force-english-output
fi
