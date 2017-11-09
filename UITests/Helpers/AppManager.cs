using System;
using Jenkins.UITests.Settings;
using Xamarin.UITest;
using Xamarin.UITest.Configuration;

namespace Jenkins.UITests.Helpers
{
    public static class AppManager
    {
        private static IApp app;
        private static Platform? platform;

        public static IApp App
        {
            get
            {
                if (app == null)
                {
                    throw new NullReferenceException("'AppManager.App' not set. Call 'AppManager.StartApp()' before trying to access it.");
                }

                return app;
            }
        }

        public static Platform Platform
        {
            get
            {
                if (platform == null)
                {
                    throw new NullReferenceException("'AppManager.Platform' not set.");
                }

                return platform.Value;
            }

            set
            {
                platform = value;
            }
        }

        public static bool IsOnIos => Platform == Platform.iOS;

        public static void StartApp()
        {
            if (platform == Platform.Android)
            {
                var configuration = ConfigureApp.Android;

                if (!string.IsNullOrWhiteSpace(AndroidSettings.ApkPath))
                {
                    configuration.ApkFile(AndroidSettings.ApkPath);
                }

                if (!string.IsNullOrWhiteSpace(AndroidSettings.DeviceSerial))
                {
                    configuration.DeviceSerial(AndroidSettings.DeviceSerial);
                }

                app = configuration.StartApp(AppDataMode.Clear);
            }
            else if (platform == Platform.iOS)
            {
                var configuration = ConfigureApp.iOS;

                if (!string.IsNullOrWhiteSpace(IosSettings.AppPath))
                {
                    configuration.AppBundle(IosSettings.AppPath);
                }

                if (!string.IsNullOrEmpty(IosSettings.DeviceIdentifier))
                {
                    configuration.DeviceIdentifier(IosSettings.DeviceIdentifier);
                }

                app = configuration.StartApp(AppDataMode.Clear);
            }
            else
            {
                throw new ArgumentException("Unsupported platform");
            }
        }
    }
}
