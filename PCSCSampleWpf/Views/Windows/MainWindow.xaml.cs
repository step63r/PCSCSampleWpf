using System.Windows;

namespace MinatoProject.PCSCSampleWpf.Views.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// デザイナー用コンストラクタ
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(ViewModels.MainWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}