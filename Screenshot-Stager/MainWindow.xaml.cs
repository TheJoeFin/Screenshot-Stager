using ColorPicker;
using Screenshot_Stager.Models;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using static Windows.Win32.PInvoke;
using Windows.Win32.Graphics.Gdi;
using System.Windows.Input;
using System.Diagnostics;
using GlobalHotKeys;
using GlobalHotKeys.Native.Types;

namespace Screenshot_Stager;

public partial class MainWindow : Window
{
    private HGDIOBJ hBitmap;

    public MainViewModel ViewModel { get; } = new();

    private readonly HotKeyManager hotKeyManager;

    public MainWindow()
    {
        DataContext = ViewModel;
        InitializeComponent();

        ColorPicker.Color.RGB_R = 0;
        ColorPicker.Color.RGB_G = 0;
        ColorPicker.Color.RGB_B = 139;

        hotKeyManager = new();
        IRegistration registrations = hotKeyManager.Register(VirtualKeyCode.KEY_1, Modifiers.Control | Modifiers.Shift);
        hotKeyManager.HotKeyPressed.Subscribe(_ => ViewModel.TakeScreenshot());
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ViewModel.StageSelectedWindow();
    }

    private void Window_LocationChanged(object sender, EventArgs e)
    {
        ViewModel.StageSelectedWindow();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        ViewModel.ResetTopmost();
        DeleteObject(hBitmap);
        hotKeyManager.Dispose();
        Application.Current.Shutdown();
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        Topmost = false;
        ViewModel.ResetTopmost();
    }

    private void Window_Activated(object sender, EventArgs e)
    {
        if (!SettingsExpander.IsExpanded)
            ViewModel.StageSelectedWindow();
    }

    private void PortableColorPicker_ColorChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PortableColorPicker picker)
            return;

        ViewModel.SelectedColor = new SolidColorBrush(
            System.Windows.Media.Color.FromArgb(
                (byte)picker.Color.A,
                (byte)picker.Color.RGB_R,
                (byte)picker.Color.RGB_G,
                (byte)picker.Color.RGB_B));
    }

    private void Expander_Expanded(object sender, RoutedEventArgs e)
    {
        ViewModel.ResetTopmost();
        Topmost = true;
    }

    private void SettingsExpander_Collapsed(object sender, RoutedEventArgs e)
    {
        Topmost = false;
        ViewModel.StageSelectedWindow();
    }

    private void WindowListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        ViewModel.StageSelectedWindow();
    }

    private void WindowListView_DropDownOpened(object sender, EventArgs e)
    {
        ViewModel.GetAndListWindowsCommand.Execute(null);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.SetWindowSizeText();
    }

    private void Image_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // get the data from the data context of the item that was clicked
        if (sender is not System.Windows.Controls.Image element)
            return;

        if (element.DataContext is not string tempPath)
            return;

        if (e.ClickCount == 2)
        {
            // open the file with the default program
            Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
            return;
        }

        Bitmap bitmap = new(tempPath);
        hBitmap = (HGDIOBJ)bitmap.GetHbitmap();

        try
        {
            // DoDragDrop with file thumbnail as drag image
            System.Runtime.InteropServices.ComTypes.IDataObject dataObject = DragDataObject.FromFile(tempPath);
            dataObject.SetDragImage(hBitmap, 100, 100);
            DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);
        }
        catch
        {
            // DoDragDrop without drag image
            IDataObject dataObject = new DataObject(DataFormats.FileDrop, new[] { tempPath });
            DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);
        }
    }
}
