namespace NepaliDatePickerDemo;

public partial class App : Application
{
    private readonly MainPage _MainPage;

    public App(MainPage mainPage)
    {
        InitializeComponent();
        _MainPage = mainPage;
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new(new NavigationPage(_MainPage));
}
