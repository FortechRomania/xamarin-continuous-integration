#!/usr/bin/env bash

# Dependencies
# ANDROID_HOME defined with the location of the Android SDK installation
# http://xmlstar.sourceforge.net/ installed
# /Library/Frameworks/Mono.framework/Commands added to the PATH variable
# /usr/local/bin/ added to the PATH variable
# ANDROID_HOME/platform-tools/ added to the PATH variable

set -xe

usage() {
    echo "Usage: run_android_tests.sh FULL_SOLUTION_ROOT FULL_APK_PATH FULL_AVD_PATH AVD_NAME API_LEVEL"
    echo "Jenkins Usage Example: ./UITests/Scripts/run_android_tests.sh \$WORKSPACE \$WORKSPACE/APK/Droid/bin/Release/cosmin.stirbu.Jenkins-signed.apk /Users/cosminstirbu/.android/avd UITestsEmulator 24"
}

if [ $# -ne 5 ]; then
    usage
    exit 1
fi

SOLUTION_ROOT=$1
APK_PATH=$2
AVD_PATH=$3
AVD_NAME=$4
API_LEVEL=$5

cd "$SOLUTION_ROOT"

"$SOLUTION_ROOT/UITests/Scripts/create_and_launch_avd.sh" $SOLUTION_ROOT $AVD_PATH ${AVD_NAME}_${API_LEVEL} $API_LEVEL

# Give time to the emulator to power on
sleep 60

DEVICE_SERIAL=($(adb devices | head -n 2 | awk '{if (NR!=1) {print $1};}'))
adb -s $DEVICE_SERIAL shell settings put secure show_ime_with_hard_keyboard 0

xml ed --inplace --update "/root/data[@name='DeviceSerial']/value" --value "$DEVICE_SERIAL" "$SOLUTION_ROOT/UITests/Settings/AndroidSettings.resx"
xml ed --inplace --update "/root/data[@name='ApkPath']/value" --value "$APK_PATH" "$SOLUTION_ROOT/UITests/Settings/AndroidSettings.resx"
xml ed --inplace --update "/root/data[@name='Platform']/value" --value "Android" "$SOLUTION_ROOT/UITests/Settings/GlobalSettings.resx"

xml ed --inplace --delete "/_:Project/_:ItemGroup/_:ProjectReference" "$SOLUTION_ROOT/UITests/Jenkins.UITests.csproj"

nuget restore "$SOLUTION_ROOT/Jenkins.sln"
nuget install NUnit.ConsoleRunner -Version 3.7.0

msbuild /t:Build /p:Configuration=Debug "$SOLUTION_ROOT/UITests/Jenkins.UITests.csproj"
mono "$SOLUTION_ROOT/NUnit.ConsoleRunner.3.7.0/tools/nunit3-console.exe" "$SOLUTION_ROOT/UITests/bin/Debug/UITests.dll"