using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NepaliUtility.Models;

namespace NepaliDatePickerDemo;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLight), nameof(IsDark), nameof(IsSystem))]
    private AppTheme _Theme = AppTheme.Unspecified;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEnglish), nameof(IsNepali))]
    private DateDisplayMode _DisplayMode = DateDisplayMode.Both;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBottomSheet), nameof(IsDialog))]
    private PickerPresentation _Presentation = PickerPresentation.BottomSheet;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCalendar), nameof(IsWheel), nameof(IsInput))]
    private PickerStyle _PickerStyle = PickerStyle.Calendar;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNativeTheme), nameof(IsNotNativeTheme))]
    private bool _UseNativeTheme = false;

    public bool IsLight         => Theme == AppTheme.Light;
    public bool IsDark          => Theme == AppTheme.Dark;
    public bool IsSystem        => Theme == AppTheme.Unspecified;
    public bool IsEnglish       => DisplayMode == DateDisplayMode.Both;
    public bool IsNepali        => DisplayMode == DateDisplayMode.BsOnly;
    public bool IsBottomSheet   => Presentation == PickerPresentation.BottomSheet;
    public bool IsDialog        => Presentation == PickerPresentation.Dialog;
    public bool IsCalendar      => PickerStyle == PickerStyle.Calendar;
    public bool IsWheel         => PickerStyle == PickerStyle.Wheel;
    public bool IsInput         => PickerStyle == PickerStyle.Input;
    public bool IsNativeTheme   => UseNativeTheme;
    public bool IsNotNativeTheme => !UseNativeTheme;

    partial void OnThemeChanged(AppTheme value)
    {
        if (Application.Current is not null)
            Application.Current.UserAppTheme = value;
    }

    [RelayCommand] void SetThemeLight()      => Theme = AppTheme.Light;
    [RelayCommand] void SetThemeDark()       => Theme = AppTheme.Dark;
    [RelayCommand] void SetThemeSystem()     => Theme = AppTheme.Unspecified;
    [RelayCommand] void SetEnglish()         => DisplayMode = DateDisplayMode.Both;
    [RelayCommand] void SetNepali()          => DisplayMode = DateDisplayMode.BsOnly;
    [RelayCommand] void SetBottomSheet()     => Presentation = PickerPresentation.BottomSheet;
    [RelayCommand] void SetDialog()          => Presentation = PickerPresentation.Dialog;
    [RelayCommand] void SetCalendar()        => PickerStyle = PickerStyle.Calendar;
    [RelayCommand] void SetWheel()           => PickerStyle = PickerStyle.Wheel;
    [RelayCommand] void SetInput()           => PickerStyle = PickerStyle.Input;
    [RelayCommand] void EnableNativeTheme()  => UseNativeTheme = true;
    [RelayCommand] void DisableNativeTheme() => UseNativeTheme = false;
}
