#!/usr/bin/env bash

# Dependencies
# /Library/Frameworks/Mono.framework/Commands added to the PATH variable

set -xe

usage() {
    echo "Usage: build_app.sh FULL_SOLUTION_ROOT CONFIGURATION"
    echo "Jenkins Usage Example: ./iOS/Scripts/build_app.sh \$WORKSPACE Debug"
}

if [ $# -ne 2 ]; then
    usage
    exit 1
fi

SOLUTION_ROOT=$1
CONFIGURATION=$2

cd "$SOLUTION_ROOT"

nuget restore "$SOLUTION_ROOT/Jenkins.sln"
msbuild /p:Configuration=$CONFIGURATION /p:Platform="iPhoneSimulator" "$SOLUTION_ROOT/iOS/Jenkins.iOS.csproj"

CURRENT_DIR="$PWD"
OUTPUT="$SOLUTION_ROOT/iOS/bin/iPhoneSimulator/$CONFIGURATION"

cd "$OUTPUT"

zip -r "$OUTPUT/Jenkins-$CONFIGURATION-Simulator.zip" Jenkins.iOS.app

cd "$CURRENT_DIR"
