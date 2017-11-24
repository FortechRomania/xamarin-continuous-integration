// Loads our list of addins from the tools folder
// this assumes using the new bootstrapper build.sh in the root folder
// which downloads the required files
#load "./tools/addins.cake"
#load "./tools/utils.cake"
#load "./tools/ios_utils.cake"
#load "./tools/android_utils.cake"

using System.Threading;
using System.Diagnostics;

// From Cake.Xamarin.Build, dumps out versions of things
LogSystemInfo ();

// GENERAL ARGUMENTS
var TARGET = Argument("target", "Build");
var CONFIGURATION = Argument("configuration", "Release");
var HOME = EnvironmentVariable("HOME");

// ANDROID APK ARGUMENTS
var BUILD_TOOLS = "27.0.1";
var ANDROID_HOME = EnvironmentVariable("ANDROID_HOME");
var KEYSTORE = $"{Environment.CurrentDirectory}/Droid/Scripts/ProductionKey.keystore";
var KEYSTORE_ALIAS = "productionkey";
var STORE_PASS = Argument("storepass", "myproductionpassword");
var ZIP_ALIGN = $"{ANDROID_HOME}/build-tools/{BUILD_TOOLS}/zipalign";
var APK_SIGNER = $"{ANDROID_HOME}/build-tools/{BUILD_TOOLS}/apksigner";


// IOS UI TESTS ARGUMENTS
var APP_PATH = Argument("app-path", "");
var SIMULATOR_NAME = Argument("sim-name", "UITests_Iphone6_11.1");
var DEVICE_TYPE = Argument("device-type", "com.apple.CoreSimulator.SimDeviceType.iPhone-6");
var DEVICE_RUNTIME = Argument("device-type", "com.apple.CoreSimulator.SimRuntime.iOS-11-1");

// ANDROID UI TESTS ARGUMENTS
var AVD_HOME = $"{HOME}/.android/avd";
var APK_PATH = Argument("apk-path", "");
var AVD_NAME = Argument("avd-name", "UITestsEmulator");
var API_LEVEL = Argument("emulator-api-level", "24");
var AVD_MANAGER = $"{ANDROID_HOME}/tools/bin/avdmanager";
var EMULATOR = $"{ANDROID_HOME}/tools/emulator";

// PROJECTS

public static class Projects
{
    public static string Solution = "Jenkins.sln";
    public static string iOSPath = "./iOS";
    public static string iOS = $"{iOSPath}/Jenkins.iOS.csproj";
    public static string AndroidPath = "./Droid";
    public static string Android = $"{AndroidPath}/Jenkins.Droid.csproj";
    public static string UnitTestsPath = "./UnitTests";
    public static string UnitTests = $"{UnitTestsPath}/Jenkins.UnitTests.csproj";
    public static string UITestsPath = "./UITests";
    public static string UITests = $"{UITestsPath}/Jenkins.UITests.csproj";
}

public static class ApplicationsInfo
{
    public static string AndroidPackageName = "cosmin.stirbu.Jenkins";
    public static string iOSAppName = "Jenkins.iOS";
}

// TASKS

Task("NuGetRestore")
.Does(() => {
    NuGetRestore(Projects.Solution);
});

Task("Build")
.IsDependentOn("NuGetRestore")
.Does(() => {
    MSBuild(Projects.Solution);
});

Task("UnitTests")
.Does(() => {
    MSBuild(Projects.UnitTests);
    NUnit3($"{Projects.UnitTestsPath}/bin/**/*Tests.dll");
});

Task("SpecFlowDefinitionCheck")
.Does(() => {
    NuGetRestore(Projects.Solution);
    ClearProjectDependenciesForProject(Projects.UITests);
    MSBuild(Projects.UITestsPath);
    SpecFlowStepDefinitionReport(Projects.UITests);
});

Task("HealthCheck")
.IsDependentOn("Build")
.IsDependentOn("UnitTests");
//.IsDependentOn("SpecFlowDefinitionCheck");

Task("iOSUITests")
.IsDependentOn("NuGetRestore")
.Does(() => {
    var simulator = CreateAppleSimulatorIfNeeded(SIMULATOR_NAME, DEVICE_TYPE, DEVICE_RUNTIME);
    DisableiOSKeyboardSettings(simulator);

    XmlPoke($"{Projects.UITestsPath}/Settings/IosSettings.resx", "/root/data[@name='DeviceIdentifier']/value", simulator.UDID);
    XmlPoke($"{Projects.UITestsPath}/Settings/IosSettings.resx", "/root/data[@name='AppPath']/value", APP_PATH);
    XmlPoke($"{Projects.UITestsPath}/Settings/GlobalSettings.resx", "/root/data[@name='Platform']/value", "iOS");

    ClearProjectDependenciesForProject(Projects.UITests);

    MSBuild(Projects.UITests, new MSBuildSettings().SetConfiguration("Debug"));
    NUnit3($"{Projects.UITestsPath}/bin/**/*Tests.dll");
});

Task("AndroidUITests")
.IsDependentOn("NuGetRestore")
.Does(() => {
    var fullAvdName = $"{AVD_NAME}_{API_LEVEL}";
    CreateAndroidEmulator(API_LEVEL, fullAvdName);
    var emulatorProcess = LaunchAndroidEmulator(fullAvdName);
    var deviceSerial = AdbDevices().First().Serial;

    AdbShell("settings put secure show_ime_with_hard_keyboard 0", new AdbToolSettings { Serial = deviceSerial });

    XmlPoke($"{Projects.UITestsPath}/Settings/AndroidSettings.resx", "/root/data[@name='DeviceSerial']/value", deviceSerial);
    XmlPoke($"{Projects.UITestsPath}/Settings/AndroidSettings.resx", "/root/data[@name='ApkPath']/value", APK_PATH);
    XmlPoke($"{Projects.UITestsPath}/Settings/GlobalSettings.resx", "/root/data[@name='Platform']/value", "Android");

    ClearProjectDependenciesForProject(Projects.UITests);
    
    MSBuild(Projects.UITests, new MSBuildSettings().SetConfiguration("Debug"));
    NUnit3($"{Projects.UITestsPath}/bin/**/*Tests.dll");

    emulatorProcess.Kill();
});

Task("BuildSimulatorApp")
.IsDependentOn("NuGetRestore")
.Does(() => {
    var buildSettings = new MSBuildSettings()
                            .WithProperty("Platform", "iPhoneSimulator")
                            .SetConfiguration(CONFIGURATION);
    MSBuild(Projects.iOS, buildSettings);
    
    var msBuildOutputDir = $"{Projects.iOSPath}/bin/iPhoneSimulator/{CONFIGURATION}";
    ZipDirectory(sourceDirectoryPath: $"{msBuildOutputDir}/{ApplicationsInfo.iOSAppName}.app",
                 sourceDirectoryName: $"{ApplicationsInfo.iOSAppName}.app",
                 zipFilePath: $"{msBuildOutputDir}/{ApplicationsInfo.iOSAppName}-{CONFIGURATION}.app.zip");
});

Task("BuildApk")
.IsDependentOn("NuGetRestore")
.Does(() => {
    var buildSettings = new MSBuildSettings()
                            .WithTarget("PackageForAndroid")
                            .SetConfiguration(CONFIGURATION);
    MSBuild(Projects.Android, buildSettings);

    var binFolder = $"{Environment.CurrentDirectory}/Droid/bin/{CONFIGURATION}/";

    var inputApk = $"{binFolder}/{ApplicationsInfo.AndroidPackageName}.apk";
    var alignedApk = $"{binFolder}/{ApplicationsInfo.AndroidPackageName}-aligned.apk";
    var signedApk = $"{binFolder}/{ApplicationsInfo.AndroidPackageName}-signed.apk";

    StartProcess(ZIP_ALIGN, $"-f -v 4 {inputApk} {alignedApk}");
    StartProcess(APK_SIGNER, $"sign -v --ks {KEYSTORE} --ks-key-alias {KEYSTORE_ALIAS} --ks-pass pass:{STORE_PASS} --out {signedApk} {alignedApk}");
    StartProcess(APK_SIGNER, $"verify {signedApk}");
});

Task("BuildIpa")
.IsDependentOn("NuGetRestore")
.Does(() => {
    var buildSettings = new MSBuildSettings()
                            .WithProperty("Platform", "iPhone")
                            .WithProperty("IpaPackageDir", $"IPA")
                            .WithProperty("BuildIpa", "true")
                            .SetConfiguration(CONFIGURATION);
    MSBuild(Projects.iOS, buildSettings);
});

RunTarget(TARGET);