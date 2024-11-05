using System.Windows;
using static Screenshot_Stager.WindowMethods;
using System.Windows.Interop;
using System.Windows.Media;
using ColorPicker;

namespace Screenshot_Stager;

public partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new();

    public MainWindow()
    {
        DataContext = ViewModel;
        InitializeComponent();

        IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
        DWMWINDOWATTRIBUTE attribute = DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
        DWM_WINDOW_CORNER_PREFERENCE preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_DONOTROUND;
        DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));
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

    private void WindowListView_ContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
    {
        ViewModel.GetAndListWindowsCommand.Execute(null);
    }

    private void WindowListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        ViewModel.StageSelectedWindow();
    }
}
