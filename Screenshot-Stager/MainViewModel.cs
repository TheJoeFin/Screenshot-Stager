using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Screenshot_Stager;
public partial class MainViewModel : ObservableRecipient
{
    public ObservableCollection<WindowDetails> WindowList { get; set; } = [];

    [ObservableProperty]
    private WindowDetails? selectedWindow;

    [ObservableProperty]
    private double left = 50;

    [ObservableProperty]
    private double top = 30;

    [ObservableProperty]
    private double width = 1100;

    [ObservableProperty]
    private double height = 700;

    [ObservableProperty]
    private Thickness appPadding = new(50);

    [ObservableProperty]
    private bool isOptionsFlyoutOpen = true;

    private int screenshotIndex = 1;

    [ObservableProperty]
    private string filename = "screenshot-fileName";

    [ObservableProperty]
    private SolidColorBrush selectedColor = new(Colors.DarkBlue);

    [ObservableProperty]
    private string backgroundImagepath = string.Empty;

    [ObservableProperty]
    private string outputImageSizeText = string.Empty;

    private int titleBarHeight = 42;
    private int windowEdgeBuffer = 6;

    public MainViewModel()
    {
        GetAndListWindows();
        SetOutputImageSizeText();
    }

    private void SetOutputImageSizeText()
    {
        nint? windowPointer = null;
        IDictionary<nint, string> windows = WindowMethods.GetOpenWindows();

        foreach (KeyValuePair<nint, string> window in windows)
        {
            if (window.Value == "StagerWindow")
            {
                windowPointer = window.Key;
                break;
            }
        }

        if (windowPointer is null)
            return;

        double dpi = WindowMethods.GetScaleForHwnd(windowPointer.Value);
        int outputImageWidth = (int)((Width - (windowEdgeBuffer * 2)) / dpi);
        int outputImageHeight = (int)((Height - (windowEdgeBuffer * 2) - titleBarHeight) / dpi);
        OutputImageSizeText = $"Screenshot Size: {outputImageWidth} x {outputImageHeight}";
    }

    [RelayCommand]
    public void StageSelectedWindow()
    {
        if (SelectedWindow is not WindowDetails window)
            return;

        // get dpi of this window
        double dpi = WindowMethods.GetScaleForHwnd(window.Handle);
        
        int newWindowX = (int)((Left + AppPadding.Left) / dpi);
        int newWindowY = (int)((Top + AppPadding.Top + titleBarHeight) / dpi);
        int newWindowWidth = (int)((Width - (AppPadding.Left + AppPadding.Right)) / dpi);
        int newWindowHeight = (int)((Height - (AppPadding.Top + AppPadding.Bottom + titleBarHeight)) / dpi);

        WindowMethods.ChangeSize(window.Handle, newWindowX, newWindowY, newWindowWidth, newWindowHeight);

        IsOptionsFlyoutOpen = false;

        Filename = $"{window.Title}-{screenshotIndex:D2}.png";
        IncrementScreenshotIndex();
        SetOutputImageSizeText();
    }

    private void IncrementScreenshotIndex()
    {
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Stager-Files");
        string path = Path.Combine(folderPath, Filename);

        if (File.Exists(path))
        {
            screenshotIndex++;
            Filename = $"{SelectedWindow?.Title}-{screenshotIndex:D2}.png";
            IncrementScreenshotIndex();
        }
    }

    [RelayCommand]
    public void ResetTopmost()
    {
        if (SelectedWindow is not WindowDetails window)
            return;

        // get dpi of this window
        double dpi = WindowMethods.GetScaleForHwnd(window.Handle);

        int newWindowX = (int)((Left + AppPadding.Left) / dpi);
        int newWindowY = (int)((Top + AppPadding.Top + titleBarHeight) / dpi);
        int newWindowWidth = (int)((Width - (AppPadding.Left + AppPadding.Right)) / dpi);
        int newWindowHeight = (int)((Height - (AppPadding.Top + AppPadding.Bottom + titleBarHeight)) / dpi);

        WindowMethods.ChangeSize(window.Handle, newWindowX, newWindowY, newWindowWidth, newWindowHeight, false);
    }

    [RelayCommand]
    private void GetAndListWindows()
    {
        WindowList.Clear();
        IDictionary<nint, string> windows = WindowMethods.GetOpenWindows();

        foreach (KeyValuePair<nint, string> window in windows)
        {
            if (window.Value == "StagerWindow")
                continue;

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

        // get dpi of this window
        double dpi = WindowMethods.GetScaleForHwnd(window.Handle);

        int screenshotX = (int)((Left + windowEdgeBuffer) / dpi);
        int screenshotY = (int)((Top + titleBarHeight + windowEdgeBuffer) / dpi);
        int screenshotWidth = (int)((Width - (2 * windowEdgeBuffer)) / dpi);
        int screenshotHeight = (int)((Height - ((2 * windowEdgeBuffer) + titleBarHeight)) / dpi);

        // take a screenshot of the specified region
        Bitmap screenshot = GetRegionOfScreenAsBitmap(screenshotX, screenshotY, screenshotWidth, screenshotHeight);

        // save or process the screenshot as needed
        // e.g., save to file
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Stager-Files");
        string path = Path.Combine(folderPath, Filename);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        try
        {
            screenshot.Save(path, ImageFormat.Png);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving screenshot: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            screenshotIndex++;
            Filename = $"{window.Title}-{screenshotIndex:D2}.png";
        }
    }

    [RelayCommand]
    private void OpenScreenshotsFolder()
    {
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Stager-Files");

        if (!Directory.Exists(folderPath))
        {
            MessageBox.Show("No screenshots have been taken yet.", "No Screenshots", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Process.Start("explorer.exe", folderPath);
    }

    [RelayCommand]
    private void PickBackgroundImage()
    {
        OpenFileDialog openFileDialog = new()
        {
            Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*",
            Title = "Select a background image"
        };

        if (openFileDialog.ShowDialog() is true)
            BackgroundImagepath = openFileDialog.FileName;
    }

    public static Bitmap GetRegionOfScreenAsBitmap(int x, int y, int width, int height)
    {
        Bitmap bmp = new(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using Graphics g = Graphics.FromImage(bmp);

        g.CopyFromScreen(x, y, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

        return bmp;
    }
}
