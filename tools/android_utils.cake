#load "./addins.cake"
#load "../build.cake"

public void CreateAndroidEmulator(string apiLevel, string fullAvdName)
{
    var package = $"system-images;android-{apiLevel};google_apis;x86_64";

    ReplaceRegexInFiles("**/Scripts/config.ini", "AvdId=.*", $"AvdId={fullAvdName}");
    ReplaceRegexInFiles("**/Scripts/config.ini", "avd.ini.displayname=.*", $"avd.ini.displayname={fullAvdName}");
    ReplaceRegexInFiles("**/Scripts/config.ini", "android-[0-9]*", $"android-{apiLevel}");

    StartProcess(AVD_MANAGER, $"--verbose create avd --name {fullAvdName} --package \"{package}\" --force", "no");
    DeleteFile($"{AVD_HOME}/{fullAvdName}.avd/config.ini");
    CopyFile($"{Projects.UITestsPath}/Scripts/config.ini", $"{AVD_HOME}/{fullAvdName}.avd/config.ini");
}

public IProcess LaunchAndroidEmulator(string fullAvdName)
{
    var process = StartAndReturnProcess(EMULATOR, new ProcessSettings { Arguments =  $"-netdelay none -netspeed full -avd {fullAvdName}" });
    Information("Waiting for emulator to start");
    Thread.Sleep(TimeSpan.FromSeconds(60));
    return process;
}