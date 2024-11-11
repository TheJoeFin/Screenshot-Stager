using CommunityToolkit.Mvvm.ComponentModel;
using Windows.Win32.Foundation;

namespace Screenshot_Stager;

public class WindowDetails : ObservableObject
{
    internal HWND Handle { get; set; }
    public string Title { get; set; } = string.Empty;
}