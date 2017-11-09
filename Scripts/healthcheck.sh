#!/usr/bin/env bash

# Dependencies
# /Library/Frameworks/Mono.framework/Commands added to the PATH variable

set -xe

usage() {
    echo "Usage: healthcheck.sh FULL_SOLUTION_ROOT"
    echo "Jenkins Usage Example: ./Scripts/healthcheck.sh \$WORKSPACE"
}

if [ $# -ne 1 ]; then
    usage
    exit 1
fi

SOLUTION_ROOT=$1

cd "$SOLUTION_ROOT"

nuget restore "$SOLUTION_ROOT/Jenkins.sln"
msbuild /t:Build "$SOLUTION_ROOT/Jenkins.sln"

nuget install NUnit.ConsoleRunner -Version 3.7.0
mono "$SOLUTION_ROOT/NUnit.ConsoleRunner.3.7.0/tools/nunit3-console.exe" "$SOLUTION_ROOT/UnitTests/bin/Debug/UnitTests.dll"