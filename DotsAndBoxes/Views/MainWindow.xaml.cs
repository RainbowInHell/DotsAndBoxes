using System.Windows;
using DotsAndBoxes.ViewModels;

namespace DotsAndBoxes.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        DataContext = _viewModel = viewModel;
        InitializeComponent();
    }
}