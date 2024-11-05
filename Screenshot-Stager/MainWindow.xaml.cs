using System.Windows;
using static Screenshot_Stager.WindowMethods;
using System.Windows.Interop;

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
        ViewModel.ResetTopmost();
    }

    private void Window_Activated(object sender, EventArgs e)
    {
        ViewModel.StageSelectedWindow();
    }
}
