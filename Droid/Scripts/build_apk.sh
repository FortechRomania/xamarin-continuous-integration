#!/usr/bin/env bash

# Dependencies
# ANDROID_HOME defined with the location of the Android SDK installation
# /Library/Frameworks/Mono.framework/Commands added to the PATH variable

set -e

usage() {
    echo "Usage: build_apk.sh FULL_SOLUTION_ROOT CONFIGURATION STORE_PASS BUILD_TOOLS_VERSION"
    echo "Jenkins Usage Example: ./Droid/Scripts/build_apk.sh \$WORKSPACE Release myproductionpassword 26.0.2"
}

if [ $# -ne 4 ]; then
    usage
    exit 1
fi

SOLUTION_ROOT=$1
CONFIGURATION=$2
STORE_PASS=$3
BUILD_TOOLS_VERSION=$4

cd "$SOLUTION_ROOT"

nuget restore "$SOLUTION_ROOT/Jenkins.sln"
msbuild /p:Configuration=$CONFIGURATION /t:PackageForAndroid "$SOLUTION_ROOT/Droid/Jenkins.Droid.csproj"

INPUT_APK="$SOLUTION_ROOT/Droid/bin/release/cosmin.stirbu.Jenkins.apk"
ALIGNED_APK="$SOLUTION_ROOT/Droid/bin/release/cosmin.stirbu.Jenkins-aligned.apk"
SIGNED_APK="$SOLUTION_ROOT/Droid/bin/release/cosmin.stirbu.Jenkins-signed.apk"

KEYSTORE_FILE="$SOLUTION_ROOT/Droid/Scripts/ProductionKey.keystore"
KEYSTORE_ALIAS=productionkey

"$ANDROID_HOME/build-tools/$BUILD_TOOLS_VERSION/zipalign" -f -v 4 $INPUT_APK $ALIGNED_APK
"$ANDROID_HOME/build-tools/$BUILD_TOOLS_VERSION/apksigner" sign -v --ks $KEYSTORE_FILE --ks-key-alias $KEYSTORE_ALIAS --ks-pass pass:$STORE_PASS  --out $SIGNED_APK $ALIGNED_APK
"$ANDROID_HOME/build-tools/$BUILD_TOOLS_VERSION/apksigner" verify $SIGNED_APK