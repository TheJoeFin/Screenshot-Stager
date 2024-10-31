using System.Windows;

namespace Screenshot_Stager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ChangeSizeBTN_Click(object sender, RoutedEventArgs e)
    {
        // random number between 1 and 100
        int diff = new Random().Next(-100, 100);
        Height += diff;
        Width += diff;
    }

    private void ListWindows_Click(object sender, RoutedEventArgs e)
    {
        IDictionary<nint, string> windows = OpenWindowGetter.GetOpenWindows();

        foreach (KeyValuePair<nint, string> window in windows)
        {
            WindowsText.Text += window.Value + Environment.NewLine;
        }
    }
}