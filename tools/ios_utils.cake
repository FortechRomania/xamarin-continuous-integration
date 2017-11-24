#load "./addins.cake"
#load "../build.cake"

public void DisableiOSKeyboardSettings(AppleSimulator simulator)
{

    var KEYBOARD_SETTINGS = new List<String> {  "KeyboardAllowPaddle",
                                                "KeyboardAssistant",
                                                "KeyboardAutocapitalization",
                                                "KeyboardAutocorrection",
                                                "KeyboardCapsLock",
                                                "KeyboardCheckSpelling",
                                                "KeyboardPeriodShortcut",
                                                "KeyboardPrediction",
                                                "KeyboardShowPredictionBar" };

    var simulatorPreferencesPath = $"{HOME}/Library/Developer/CoreSimulator/Devices/{simulator.UDID}/data/Library/Preferences/com.apple.Preferences.plist";
    KEYBOARD_SETTINGS.ForEach( setting => {
        StartProcess("plutil", $"-replace {setting} -bool NO {simulatorPreferencesPath}");
    });
}

public AppleSimulator CreateAppleSimulatorIfNeeded(string simulatorName, string deviceType, string deviceRuntime)
{
    var simulator = ListAppleSimulators().FirstOrDefault(x => x.Name == simulatorName);

    if (simulator != null) return simulator;

    StartProcess("xcrun", $"simctl create {simulatorName} {deviceType} {deviceRuntime}");
    simulator = ListAppleSimulators().First(x => x.Name == simulatorName);
    BootAppleSimulator(simulator.UDID);
    Thread.Sleep(TimeSpan.FromSeconds(30));
    StartProcess("xcrun", $"simctl shutdown {simulator.UDID}"); 

    return simulator;
}