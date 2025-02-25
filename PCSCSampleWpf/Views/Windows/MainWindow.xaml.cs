using MinatoProject.PCSCSampleWpf.ViewModels;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Controls;

namespace MinatoProject.PCSCSampleWpf.Views.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : INavigationWindow
{
    /// <summary>
    /// ViewModelインスタンス
    /// </summary>
    public MainWindowViewModel ViewModel { get; }

    /// <summary>
    /// ViewModelの引数付きコンストラクタ
    /// </summary>
    /// <param name="viewModel">ViewModelのインスタンス</param>
    /// <param name="navigationService">INavigationService</param>
    /// <param name="snackbarService">ISnackbarService</param>
    public MainWindow(MainWindowViewModel viewModel, INavigationService navigationService, ISnackbarService snackbarService)
    {
        ViewModel = viewModel;
        DataContext = this;

        Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);

        InitializeComponent();

        navigationService.SetNavigationControl(RootNavigation);
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);
    }

    #region INavigationWindow
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public INavigationView GetNavigation() => RootNavigation;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pageType"></param>
    /// <returns></returns>
    public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="navigationViewPageProvider"></param>
    public void SetPageService(INavigationViewPageProvider navigationViewPageProvider)
        => RootNavigation.SetPageProviderService(navigationViewPageProvider);

    /// <summary>
    /// 
    /// </summary>
    public void ShowWindow() => Show();

    /// <summary>
    /// 
    /// </summary>
    public void CloseWindow() => Close();

    /// <summary>
    /// Raises the closed event.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // Make sure that closing this window will begin the process of closing the application.
        Application.Current.Shutdown();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    INavigationView INavigationWindow.GetNavigation()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }
    #endregion
}