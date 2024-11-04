using CommunityToolkit.Mvvm.ComponentModel;

namespace Screenshot_Stager;

public class WindowDetails : ObservableObject
{
    public nint Handle { get; set; }
    public string Title { get; set; } = string.Empty;
}