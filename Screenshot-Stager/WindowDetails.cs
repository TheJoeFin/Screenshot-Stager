using CommunityToolkit.Mvvm.ComponentModel;
using static Screenshot_Stager.NativeMethods;


namespace Screenshot_Stager;

public class WindowDetails : ObservableObject
{
    internal HWND Handle { get; set; }
    public string Title { get; set; } = string.Empty;
}