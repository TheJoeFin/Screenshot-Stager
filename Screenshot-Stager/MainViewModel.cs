﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using GlobalHotKeys;
using GlobalHotKeys.Native.Types;
using System.Windows.Media;
using Windows.Win32.Foundation;
using System.Text.RegularExpressions;
using System.Text;

namespace Screenshot_Stager;
public partial class MainViewModel : ObservableRecipient
{
    public ObservableCollection<WindowDetails> WindowList { get; set; } = [];

    public ObservableCollection<string> RecentCaptures { get; set; } = [];

    [ObservableProperty]
    private int recentPaneWidth = 0;

    [ObservableProperty]
    private WindowDetails? selectedWindow;

    [ObservableProperty]
    private double left = 50;

    [ObservableProperty]
    private double top = 30;

    [ObservableProperty]
    private double width = 1102;

    [ObservableProperty]
    private double height = 692;

    [ObservableProperty]
    private double outputImageWidth = 2200;

    [ObservableProperty]
    private double outputImageHeight = 1300;

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
    private string backgroundImagePath = string.Empty;

    [ObservableProperty]
    private string outputImageSizeText = string.Empty;

    readonly private int titleBarHeight = 40;
    readonly private int windowEdgeBuffer = 1;
    private bool isSettingWindowSize = false;

    public MainViewModel()
    {
        HotKeyManager hotKeyManager = new();
        IRegistration registrations = hotKeyManager.Register(VirtualKeyCode.KEY_1, Modifiers.Control | Modifiers.Shift);
        hotKeyManager.HotKeyPressed.Subscribe(_ => TakeScreenshot());

    }

    partial void OnWidthChanged(double value)
    {
        if (isSettingWindowSize)
            return;

        SetOutputImageSizeText();
    }

    partial void OnHeightChanged(double value)
    {
        if (isSettingWindowSize)
            return;

        SetOutputImageSizeText();
    }

    public void SetOutputImageSizeText()
    {
        HWND? windowPointer = null;
        IDictionary<HWND, string> windows = WindowMethods.GetOpenWindows();

        foreach (KeyValuePair<HWND, string> window in windows)
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
        OutputImageWidth = (int)((Width) / dpi);
        OutputImageHeight = (int)((Height + titleBarHeight) / dpi);
    }

    [RelayCommand]
    public void SetWindowSizeText()
    {
        HWND? windowPointer = null;
        IDictionary<HWND, string> windows = WindowMethods.GetOpenWindows();

        foreach (KeyValuePair<HWND, string> window in windows)
        {
            if (window.Value == "StagerWindow")
            {
                windowPointer = window.Key;
                break;
            }
        }

        if (windowPointer is null)
            return;

        isSettingWindowSize = true;
        double dpi = WindowMethods.GetScaleForHwnd(windowPointer.Value);
        Width = (int)Math.Round((OutputImageWidth * dpi) + (windowEdgeBuffer * 2), 0, MidpointRounding.AwayFromZero);
        Height = (int)Math.Round((OutputImageHeight * dpi) + (windowEdgeBuffer * 2) + titleBarHeight, 0, MidpointRounding.AwayFromZero);
        OutputImageSizeText = $"Screenshot Size: {Width} x {Height}";
        isSettingWindowSize = false;
    }

    [RelayCommand]
    public void StageSelectedWindow()
    {
        if (SelectedWindow is not WindowDetails window)
            return;

        // get dpi of this window
        double dpi = WindowMethods.GetScaleForHwnd(window.Handle);
        // Rect monitorRect = WindowMethods.GetMonitorRect(window.Handle);
        // int monitorX = (int)(monitorRect.X);
        // int monitorY = (int)(monitorRect.Y);

        int newWindowX = (int)((Left + AppPadding.Left) / dpi);
        int newWindowY = (int)((Top + AppPadding.Top + titleBarHeight) / dpi);
        int newWindowWidth = (int)((Width - (AppPadding.Left + AppPadding.Right)) / dpi);
        int newWindowHeight = (int)((Height - (AppPadding.Top + AppPadding.Bottom + titleBarHeight)) / dpi);

        Debug.WriteLine($"x: {newWindowX}, y: {newWindowY}, width: {newWindowWidth}, height: {newWindowHeight}");

        WindowMethods.ChangeSize(window.Handle, newWindowX, newWindowY, newWindowWidth, newWindowHeight);

        IsOptionsFlyoutOpen = false;

        Filename = $"{window.Title.MakePathSafe()}-{screenshotIndex:D2}.png";
        IncrementScreenshotIndex();
    }

    private void IncrementScreenshotIndex()
    {
        string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Stager-Files");
        string path = Path.Combine(folderPath, Filename);

        if (File.Exists(path))
        {
            screenshotIndex++;
            Filename = $"{SelectedWindow?.Title.MakePathSafe()}-{screenshotIndex:D2}.png";
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
        IDictionary<HWND, string> windows = WindowMethods.GetOpenWindows();

        foreach (KeyValuePair<HWND, string> window in windows)
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

            App.Current.Dispatcher.Invoke((Action)delegate
            {
                RecentCaptures.Insert(0, path);
                RecentPaneWidth = 50 * RecentCaptures.Count;
            });
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
    private static void OpenScreenshotsFolder()
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
            BackgroundImagePath = openFileDialog.FileName;
    }

    public static Bitmap GetRegionOfScreenAsBitmap(int x, int y, int width, int height)
    {
        Bitmap bmp = new(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using Graphics g = Graphics.FromImage(bmp);

        g.CopyFromScreen(x, y, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);

        return bmp;
    }
}
