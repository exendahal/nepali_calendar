using NepaliUtilityDemo.AspNet.Examples;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseStaticFiles();
app.MapRazorPages();

app.MapGet("/api/examples", () => Results.Ok(Example.Run()))
   .WithName("GetUtilityExamples");

app.Run();
