using System.Windows;
using DevBoxAI.ViewModels;

namespace DevBoxAI;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
