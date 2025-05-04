using Foundation;

namespace decodey_cs;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => Decodey.MauiProgram.CreateMauiApp();
}