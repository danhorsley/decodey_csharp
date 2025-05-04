namespace Decodey.Helpers
{
    public static class StatusBar
    {
        /// <summary>
        /// Sets the status bar style
        /// </summary>
        public static void SetStyle(StatusBarStyle style)
        {
#if ANDROID
            Platform.CurrentActivity.Window.DecorView.SystemUiVisibility = style == StatusBarStyle.LightContent
                ? (Android.Views.SystemUiFlags)Android.Views.StatusBarVisibility.Visible
                : (Android.Views.SystemUiFlags)(Android.Views.SystemUiFlags.LightStatusBar | Android.Views.StatusBarVisibility.Visible);
#elif IOS
            UIKit.UIApplication.SharedApplication.SetStatusBarStyle(
                style == StatusBarStyle.LightContent
                    ? UIKit.UIStatusBarStyle.LightContent
                    : UIKit.UIStatusBarStyle.DarkContent,
                false);
#endif
        }

        /// <summary>
        /// Sets the status bar color
        /// </summary>
        public static void SetColor(Color color)
        {
#if ANDROID
            Platform.CurrentActivity.Window.SetStatusBarColor(color.ToAndroid());
#elif IOS
            // iOS doesn't support directly setting the status bar color
            // The status bar takes its color from the view behind it
#endif
        }
    }

    /// <summary>
    /// Status bar style options
    /// </summary>
    public enum StatusBarStyle
    {
        DarkContent,
        LightContent
    }
}