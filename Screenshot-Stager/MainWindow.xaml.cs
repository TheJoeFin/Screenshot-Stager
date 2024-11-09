using System.Windows;
using static Screenshot_Stager.WindowMethods;
using System.Windows.Interop;
using System.Windows.Media;
using ColorPicker;
using System.Runtime.InteropServices;

namespace Screenshot_Stager;

public partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new();

    public MainWindow()
    {
        DataContext = ViewModel;
        InitializeComponent();


        // if Windows 11, remove rounded corners
        OSVERSIONINFOEX oSVERSIONINFOEX = new();
        _ = RtlGetVersion(ref oSVERSIONINFOEX);

        if (oSVERSIONINFOEX.BuildNumber >= 22000)
        {
            IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
            DWMWINDOWATTRIBUTE attribute = DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
            DWM_WINDOW_CORNER_PREFERENCE preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_DONOTROUND;
            DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));
        }

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

    [DllImport("ntdll.dll")]
    internal static extern UInt32 RtlGetVersion(ref OSVERSIONINFOEX VersionInformation);

    [StructLayout(LayoutKind.Sequential)]
    public struct OSVERSIONINFOEX
    {
        public UInt32 OSVersionInfoSize;
        public UInt32 MajorVersion;
        public UInt32 MinorVersion;
        public UInt32 BuildNumber;
        public UInt32 PlatformId;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public String CSDVersion;
        public UInt16 ServicePackMajor;
        public UInt16 ServicePackMinor;
        public UInt16 SuiteMask;
        public Byte ProductType;
        public Byte Reserved;
    }
}
