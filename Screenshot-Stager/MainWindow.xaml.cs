using System.Windows;

namespace Screenshot_Stager;

public partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new();

    public MainWindow()
    {
        DataContext = ViewModel;
        InitializeComponent();
    }


    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ViewModel.StageSelectedWindow();
    }

    private void Window_LocationChanged(object sender, EventArgs e)
    {
        ViewModel.StageSelectedWindow();
    }
}
