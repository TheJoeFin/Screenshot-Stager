using System.Windows;
using System.Windows.Media;

namespace Screenshot_Stager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        GetAndListWindows();
    }

    private void ChangeSizeBTN_Click(object sender, RoutedEventArgs e)
    {
        if (WindowList.SelectedItem is not WindowDetails window)
            return;

        int thisX = (int)Left;
        int thisY = (int)Top;
        int thisWidth = (int)Width;
        int thisHeight = (int)Height;

        // get dpi of this window
        double dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;

        int newWindowX = (int)((thisX + 50) * dpi);
        int newWindowY = (int)((thisY + 50) * dpi);
        int newWindowWidth = (int)((thisWidth - 100) * dpi);
        int newWindowHeight = (int)((thisHeight - 100) * dpi);

        WindowMethods.ChangeSize(window.Handle, newWindowX, newWindowY, newWindowWidth, newWindowHeight);
    }

    private void ListWindows_Click(object sender, RoutedEventArgs e)
    {
        GetAndListWindows();
    }

    private void GetAndListWindows()
    {
        WindowList.Items.Clear();
        IDictionary<nint, string> windows = WindowMethods.GetOpenWindows();

        foreach (KeyValuePair<nint, string> window in windows)
        {
            WindowDetails details = new()
            {
                Handle = window.Key,
                Title = window.Value
            };
            WindowList.Items.Add(details);
        }
    }

    private void SetSizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (WidthTB.Text is not string widthString 
            || HeightTB.Text is not string heightString
            || !int.TryParse(widthString, out int widthInt)
            || !int.TryParse(heightString, out int heightInt))
            return;

        double dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;

        Height = heightInt * dpi;
        Width = widthInt * dpi;
    }
}

public record WindowDetails
{
    public nint Handle { get; set; }
    public string Title { get; set; } = string.Empty;
}