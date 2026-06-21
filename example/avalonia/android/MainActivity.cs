using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using NepaliDatePickerDemo.Avalonia;

namespace NepaliDatePickerDemo.Avalonia.Android;

[Activity(
    Label = "NepaliDatePickerDemo",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        => base.CustomizeAppBuilder(builder);
}
