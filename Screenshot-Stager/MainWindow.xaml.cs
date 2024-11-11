using ColorPicker;
using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace Screenshot_Stager;

public partial class MainWindow : FluentWindow
{
    public MainViewModel ViewModel { get; } = new();

    public MainWindow()
    {
        DataContext = ViewModel;
        InitializeComponent();

        ColorPicker.Color.RGB_R = 0;
        ColorPicker.Color.RGB_G = 0;
        ColorPicker.Color.RGB_B = 139;
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ViewModel.StageSelectedWindow();
    }

    private void Window_LocationChanged(object sender, EventArgs e)
    {
        ViewModel.StageSelectedWindow();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        ViewModel.ResetTopmost();
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        Topmost = false;
        ViewModel.ResetTopmost();
    }

    private void Window_Activated(object sender, EventArgs e)
    {
        if (!SettingsExpander.IsExpanded)
            ViewModel.StageSelectedWindow();
    }

    private void PortableColorPicker_ColorChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PortableColorPicker picker)
            return;

        ViewModel.SelectedColor = new SolidColorBrush(
            Color.FromArgb(
                (byte)picker.Color.A,
                (byte)picker.Color.RGB_R,
                (byte)picker.Color.RGB_G,
                (byte)picker.Color.RGB_B));
    }

    private void Expander_Expanded(object sender, RoutedEventArgs e)
    {
        ViewModel.ResetTopmost();
        Topmost = true;
    }

    private void SettingsExpander_Collapsed(object sender, RoutedEventArgs e)
    {
        Topmost = false;
        ViewModel.StageSelectedWindow();
    }

    private void WindowListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        ViewModel.StageSelectedWindow();
    }

    private void WindowListView_DropDownOpened(object sender, EventArgs e)
    {
        ViewModel.GetAndListWindowsCommand.Execute(null);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.SetWindowSizeText();
    }
}
