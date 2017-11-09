#!/usr/bin/env bash

# Dependencies
# /Library/Frameworks/Mono.framework/Commands added to the PATH variable

set -xe

usage() {
    echo "Usage: build_ipa.sh FULL_SOLUTION_ROOT CONFIGURATION"
    echo "Jenkins Usage Example: ./iOS/Scripts/build_ipa.sh \$WORKSPACE Release"
}

if [ $# -ne 2 ]; then
    usage
    exit 1
fi

SOLUTION_ROOT=$1
CONFIGURATION=$2

cd "$SOLUTION_ROOT"

nuget restore "$SOLUTION_ROOT/Jenkins.sln"
msbuild /p:Configuration=$CONFIGURATION /p:Platform="iPhone" /p:IpaPackageDir="$SOLUTION_ROOT/iOS/IPA" /p:BuildIpa=true /t:Build "$SOLUTION_ROOT/iOS/Jenkins.iOS.csproj"