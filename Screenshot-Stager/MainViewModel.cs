using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows;

namespace Screenshot_Stager;
public partial class MainViewModel : ObservableRecipient
{
    public ObservableCollection<WindowDetails> WindowList { get; set; } = [];

    [ObservableProperty]
    private WindowDetails? selectedWindow;

    [ObservableProperty]
    private double left = 151;

    [ObservableProperty]
    private double top = 101;

    [ObservableProperty]
    private double width = 1001;

    [ObservableProperty]
    private double height = 601;

    [ObservableProperty]
    private Thickness appPadding = new(50);

    [ObservableProperty]
    private bool isOptionsFlyoutOpen = true;

    private int screenshotIndex = 1;

    public MainViewModel()
    {
        GetAndListWindows();
    }

    [RelayCommand]
    public void StageSelectedWindow()
    {
        if (SelectedWindow is not WindowDetails window)
            return;

        int titleBarHeight = 42;

        // get dpi of this window
        double dpi = WindowMethods.GetScaleForHwnd(window.Handle);
        
        int newWindowX = (int)((Left + AppPadding.Left) / dpi);
        int newWindowY = (int)((Top + AppPadding.Top + titleBarHeight) / dpi);
        int newWindowWidth = (int)((Width - (AppPadding.Left + AppPadding.Right)) / dpi);
        int newWindowHeight = (int)((Height - (AppPadding.Top + AppPadding.Bottom) - titleBarHeight) / dpi);

        WindowMethods.ChangeSize(window.Handle, newWindowX, newWindowY, newWindowWidth, newWindowHeight);

        IsOptionsFlyoutOpen = false;
    }

    [RelayCommand]
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

    [RelayCommand]
    private void TakeScreenshot()
    {
        if (SelectedWindow is not WindowDetails window)
            return;

        int titleBarHeight = 42;

        // get dpi of this window
        double dpi = WindowMethods.GetScaleForHwnd(window.Handle);

        int gapsPadding = 7;

        int screenshotX = (int)((Left + gapsPadding) / dpi);
        int screenshotY = (int)((Top + titleBarHeight + gapsPadding) / dpi);
        int screenshotWidth = (int)((Width - (2 * gapsPadding)) / dpi);
        int screenshotHeight = (int)((Height - ((2 * gapsPadding) + titleBarHeight)) / dpi);

        // take a screenshot of the specified region
        Bitmap screenshot = GetRegionOfScreenAsBitmap(screenshotX, screenshotY, screenshotWidth, screenshotHeight);

        // save or process the screenshot as needed
        // e.g., save to file
        string fileName = $"{window.Title}-{screenshotIndex:D2}.bmp";
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), fileName);

        try
        {
            screenshot.Save(path);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving screenshot: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            screenshotIndex++;
        }

    }

    public static Bitmap GetRegionOfScreenAsBitmap(int x, int y, int width, int height)
    {
        Bitmap bmp = new(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using Graphics g = Graphics.FromImage(bmp);

        g.CopyFromScreen(x, y, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

        return bmp;
    }
}
