using Jenkins.UITests.Helpers;
using Xamarin.UITest;

namespace Jenkins.UITests.Pages
{
    public class BasePage
    {
        protected IApp App => AppManager.App;

        protected bool IsOnIos => AppManager.IsOnIos;
    }
}
