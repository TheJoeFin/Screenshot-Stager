using System.Windows;
using System.Windows.Media;

namespace Screenshot_Stager;

public partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new();

    public MainWindow()
    {
        DataContext = ViewModel;
        InitializeComponent();
    }

    private void ListWindows_Click(object sender, RoutedEventArgs e)
    {
        // GetAndListWindows();
    }

    private void SetSizeButton_Click(object sender, RoutedEventArgs e)
    {
        // if (WidthTB.Text is not string widthString
        //     || HeightTB.Text is not string heightString
        //     || !int.TryParse(widthString, out int widthInt)
        //     || !int.TryParse(heightString, out int heightInt))
        //     return;
        // 
        // double dpi = VisualTreeHelper.GetDpi(this).DpiScaleX;
        // 
        // Height = heightInt;
        // Width = widthInt;
    }

    private void ChangeSizeBTN_Click(object sender, RoutedEventArgs e)
    {
        // StageSelectedWindow();
    }    

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // StageSelectedWindow();
    }

    private void Window_LocationChanged(object sender, EventArgs e)
    {
        // StageSelectedWindow();
    }
}
