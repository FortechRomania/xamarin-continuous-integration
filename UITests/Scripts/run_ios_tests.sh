#!/usr/bin/env bash

# Dependencies
# http://xmlstar.sourceforge.net/ installed
# /Library/Frameworks/Mono.framework/Commands added to the PATH variable
# /usr/local/bin/ added to the PATH variable

set -xe

usage() {
    echo "Usage: run_ios_tests.sh FULL_SOLUTION_ROOT APP_PATH SIM_NAME DEVICE_TYPE DEVICE_RUNTIME"
    echo "Jenkins Usage Example: ./UITests/Scripts/run_ios_tests.sh \$WORKSPACE \$WORKSPACE/UITestsApp/Jenkins.iOS.app UITests_Iphone6_10.3 com.apple.CoreSimulator.SimDeviceType.iPhone-6 com.apple.CoreSimulator.SimRuntime.iOS-10-3"
}

disable_keyboard_helpers() {
    for KEYBOARD_SETTING in "KeyboardAllowPaddle" "KeyboardAssistant" "KeyboardAutocapitalization" "KeyboardAutocorrection" "KeyboardCapsLock" "KeyboardCheckSpelling" "KeyboardPeriodShortcut" "KeyboardPrediction" "KeyboardShowPredictionBar";  do 
        plutil -replace $KEYBOARD_SETTING -bool NO $HOME/Library/Developer/CoreSimulator/Devices/$DEVICE_ID/data/Library/Preferences/com.apple.Preferences.plist
    done
}

if [ $# -ne 5 ]; then
    usage
    exit 1
fi

SOLUTION_ROOT=$1
APP_PATH=$2
SIM_NAME=$3
DEVICE_TYPE=$4
DEVICE_RUNTIME=$5

cd "$SOLUTION_ROOT"

DEVICE_ID=$(xcrun instruments -s devices | sed -n "/${SIM_NAME}/p" | head -n 1 | grep -o "\[.*\]" | sed -e "s/\]//g ; s/\[//g")

if [ -z "$DEVICE_ID" ]; then
    DEVICE_ID=$(xcrun simctl create ${SIM_NAME} ${DEVICE_TYPE} ${DEVICE_RUNTIME})
    xcrun simctl boot $DEVICE_ID
    sleep 30
    xcrun simctl shutdown $DEVICE_ID
fi

disable_keyboard_helpers

xml ed --inplace --update "/root/data[@name='DeviceIdentifier']/value" --value "$DEVICE_ID" "$SOLUTION_ROOT/UITests/Settings/IosSettings.resx"
xml ed --inplace --update "/root/data[@name='AppPath']/value" --value "$APP_PATH" "$SOLUTION_ROOT/UITests/Settings/IosSettings.resx"
xml ed --inplace --update "/root/data[@name='Platform']/value" --value "iOS" "$SOLUTION_ROOT/UITests/Settings/GlobalSettings.resx"

xml ed --inplace --delete "/_:Project/_:ItemGroup/_:ProjectReference" "$SOLUTION_ROOT/UITests/Jenkins.UITests.csproj"

nuget restore "$SOLUTION_ROOT/Jenkins.sln"
nuget install NUnit.ConsoleRunner -Version 3.7.0

msbuild /t:Build /p:Configuration=Debug "$SOLUTION_ROOT/UITests/Jenkins.UITests.csproj"
mono "$SOLUTION_ROOT/NUnit.ConsoleRunner.3.7.0/tools/nunit3-console.exe" "$SOLUTION_ROOT/UITests/bin/Debug/UITests.dll"