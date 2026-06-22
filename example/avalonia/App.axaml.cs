using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NepaliDatePickerDemo.Avalonia.Views;

namespace NepaliDatePickerDemo.Avalonia;

public class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow = new MainWindow();
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
                singleView.MainView = new MainView();
        }
        catch (Exception ex)
        {
            // Surface the real .NET exception so it appears in logcat / the
            // debugger "InnerException" rather than the opaque JavaProxyThrowable.
            Debug.WriteLine($"[NepaliDatePicker] Startup error: {ex}");
            throw;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
