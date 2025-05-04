using Android.App;
using Android.Runtime;

namespace Decodeys;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => decodey_cs.MauiProgram.CreateMauiApp();
}