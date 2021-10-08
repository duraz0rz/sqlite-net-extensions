using System.Reflection;
using Foundation;
using MonoTouch.NUnit.UI;
using UIKit;

namespace SQLiteNetExtensions.IntegrationTests.Touch
{
    [Foundation.Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        private UIWindow _window;
        private TouchRunner _runner;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            SQLitePCL.Batteries_V2.Init();
            _window = new UIWindow(UIScreen.MainScreen.Bounds);
            _runner = new TouchRunner(_window);

            _runner.Add(Assembly.GetExecutingAssembly());
            _window.RootViewController = new UINavigationController(_runner.GetViewController());

            _window.MakeKeyAndVisible();

            return true;
        }
    }
}
