using Microsoft.AspNetCore.Mvc.RazorPages;
using NepaliUtilityDemo.AspNet.Examples;

namespace NepaliUtilityDemo.AspNet.Pages;

public class IndexModel : PageModel
{
    public IReadOnlyList<ExampleSection> Sections { get; private set; } = Array.Empty<ExampleSection>();

    public void OnGet() => Sections = Example.Run();
}
