using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Browser;
using NepaliDatePickerDemo.Avalonia;

[assembly: SupportedOSPlatform("browser")]

internal sealed partial class Program
{
    private static async Task Main(string[] args) =>
        await AppBuilder.Configure<App>()
            .StartBrowserAppAsync("out");
}
