using Foundation;
using Avalonia;
using Avalonia.iOS;
using NepaliDatePickerDemo.Avalonia;

namespace NepaliDatePickerDemo.Avalonia.iOS;

[Register("AppDelegate")]
public class AppDelegate : AvaloniaAppDelegate<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        => base.CustomizeAppBuilder(builder);
}
