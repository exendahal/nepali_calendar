using Avalonia.Controls;
using NepaliDatePickerDemo.Avalonia.Views;

namespace NepaliDatePickerDemo.Avalonia;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Content = new MainView();
    }
}
