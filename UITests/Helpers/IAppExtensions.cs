using System.Linq;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Jenkins.UITests.Helpers
{
    public static class IAppExtensions
    {
        public static AppRect ScreenRect(this IApp app)
        {
            var window = app.Query().FirstOrDefault();

            return window.Rect;
        }

        public static void ScrollDownByDragging(this IApp app)
        {
            var windowRect = app.ScreenRect();
            var x = windowRect.CenterX;

            var startY = (windowRect.CenterY / 2) + 200;
            var endY = (windowRect.CenterY / 2) - 200;

            app.DragCoordinates(x, startY, x, endY);
        }
    }
}
