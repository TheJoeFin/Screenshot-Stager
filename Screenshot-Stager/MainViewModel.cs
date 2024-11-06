﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
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
    private SolidColorBrush selectedColor = new(Colors.Black);

    [ObservableProperty]
    private string backgroundImagepath = string.Empty;

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
        int newWindowHeight = (int)((Height - (AppPadding.Top + AppPadding.Bottom + titleBarHeight)) / dpi);

        WindowMethods.ChangeSize(window.Handle, newWindowX, newWindowY, newWindowWidth, newWindowHeight);

        IsOptionsFlyoutOpen = false;

        Filename = $"{window.Title}-{screenshotIndex:D2}.png";
        IncrementScreenshotIndex();
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

        int titleBarHeight = 42;

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

        int titleBarHeight = 42;

        // get dpi of this window
        double dpi = WindowMethods.GetScaleForHwnd(window.Handle);

        int gapsPadding = 6;

        int screenshotX = (int)((Left + gapsPadding) / dpi);
        int screenshotY = (int)((Top + titleBarHeight + gapsPadding) / dpi);
        int screenshotWidth = (int)((Width - (2 * gapsPadding)) / dpi);
        int screenshotHeight = (int)((Height - ((2 * gapsPadding) + titleBarHeight)) / dpi);

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
