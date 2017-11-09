using System;
using Jenkins.UITests.Helpers;
using Jenkins.UITests.Settings;
using SpecNuts;
using SpecNuts.Json;
using TechTalk.SpecFlow;
using Xamarin.UITest;

namespace Jenkins.UITests.Steps
{
    [Binding]
    public class Hooks
    {
        [BeforeScenario]
        public void BeforeScenario()
        {
            AppManager.Platform = GlobalSettings.Platform == "iOS" ? Platform.iOS : Platform.Android;
            AppManager.StartApp();
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            Reporters.Add(new JsonReporter());

            Reporters.FinishedReport += (sender, args) =>
            {
                var reportsPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                args.Reporter.WriteToFile($"{reportsPath}/Reports_{DateTime.Now.ToUnixTimestampUtc()}.json");
            };
        }
    }
}
