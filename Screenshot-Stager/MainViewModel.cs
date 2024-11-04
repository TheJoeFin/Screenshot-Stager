using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;

namespace Screenshot_Stager;
public partial class MainViewModel : ObservableRecipient
{
    ObservableCollection<WindowDetails> WindowList = new();

    [ObservableProperty]
    private WindowDetails? selectedWindow;

    [ObservableProperty]
    private double left;

    [ObservableProperty]
    private double top;

    [ObservableProperty]
    private double width;

    [ObservableProperty]
    private double height;

    private void StageSelectedWindow()
    {
        if (selectedWindow is not WindowDetails window)
            return;

        int thisX = (int)Left;
        int thisY = (int)Top;
        int thisWidth = (int)Width;
        int thisHeight = (int)Height;

        // get dpi of this window
        // double dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
        // 
        // int newWindowX = (int)((thisX + 50) * dpi);
        // int newWindowY = (int)((thisY + 50) * dpi);
        // int newWindowWidth = (int)((thisWidth - 100) * dpi);
        // int newWindowHeight = (int)((thisHeight - 100) * dpi);

        // WindowMethods.ChangeSize(window.Handle, newWindowX, newWindowY, newWindowWidth, newWindowHeight);
    }

    private void GetAndListWindows()
    {
        WindowList.Clear();
        IDictionary<nint, string> windows = WindowMethods.GetOpenWindows();

        foreach (KeyValuePair<nint, string> window in windows)
        {
            WindowDetails details = new()
            {
                Handle = window.Key,
                Title = window.Value
            };
            WindowList.Add(details);
        }
    }
}
