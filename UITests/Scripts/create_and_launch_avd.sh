#!/usr/bin/env bash

# Dependencies
# ANDROID_HOME defined with the location of the Android SDK installation
# ANDROID_HOME/platform-tools/ added to the PATH variable

set -xe

usage() {
    echo "Usage: create_and_launch.sh FULL_SOLUTION_ROOT FULL_AVD_HOME AVD API_LEVEL"
}

if [ $# -ne 4 ]; then
    usage
    exit 1
fi

SOLUTION_ROOT=$1
AVD_HOME=$2
AVD=$3
API_LEVEL=$4

cd "$SOLUTION_ROOT"

if [ $API_LEVEL -eq 21 ]
then
    PACKAGE="system-images;android-21;google_apis;x86_64"
elif [ $API_LEVEL -eq 23 ]
then
    PACKAGE="system-images;android-23;google_apis;x86_64"
elif [ $API_LEVEL -eq 24 ]
then
    PACKAGE="system-images;android-24;google_apis;x86_64"
fi

sed -i '' "s/AvdId=.*/AvdId=$AVD/g" "$SOLUTION_ROOT/UITests/Scripts/config.ini"
sed -i '' "s/avd.ini.displayname=.*/avd.ini.displayname=$AVD/g" "$SOLUTION_ROOT/UITests/Scripts/config.ini"
sed -i '' "s/android-[0-9]*/android-$API_LEVEL/g" "$SOLUTION_ROOT/UITests/Scripts/config.ini"

echo no | "$ANDROID_HOME/tools/bin/avdmanager" create avd --name $AVD --package $PACKAGE --force

rm "$AVD_HOME/$AVD.avd/config.ini"
cp "$SOLUTION_ROOT/UITests/Scripts/config.ini" "$AVD_HOME/$AVD.avd/config.ini"

rm -rf "$SOLUTION_ROOT/UITests/Scripts/$AVD.txt"
nohup "$ANDROID_HOME/tools/emulator" -netdelay none -netspeed full -avd "$AVD" &> "$SOLUTION_ROOT/UITests/Scripts/$AVD.txt" &