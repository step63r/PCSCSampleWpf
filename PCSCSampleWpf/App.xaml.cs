using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MinatoProject.PCSCSampleWpf.Services;
using MinatoProject.PCSCSampleWpf.Services.Interfaces;
using MinatoProject.PCSCSampleWpf.ViewModels;
using MinatoProject.PCSCSampleWpf.Views.Pages;
using MinatoProject.PCSCSampleWpf.Views.Windows;
using NLog.Extensions.Logging;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;

namespace MinatoProject.PCSCSampleWpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    private static readonly IHost _host = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(c =>
        {
            var basePath =
                Path.GetDirectoryName(AppContext.BaseDirectory)
                ?? throw new DirectoryNotFoundException(
                    "Unable to find the base directory of the application."
                );
            _ = c.SetBasePath(basePath);
        })
        .ConfigureServices((context, services) =>
        {
            _ = services.AddNavigationViewPageProvider();

            // App Host
            _ = services.AddHostedService<ApplicationHostService>();

            // Theme manipulation
            _ = services.AddSingleton<IThemeService, ThemeService>();

            // Service containing navigation, same as INavigationWindow... but without window
            _ = services.AddSingleton<INavigationService, NavigationService>();

            // Main window with navigation
            _ = services.AddSingleton<INavigationWindow, MainWindow>();
            _ = services.AddSingleton<MainWindowViewModel>();

            // Views and ViewModels
            _ = services.AddSingleton<ReaderPage>();
            _ = services.AddSingleton<ReaderPageViewModel>();
            _ = services.AddSingleton<WriterPage>();
            _ = services.AddSingleton<WriterPageViewModel>();

            // Services
            _ = services.AddSingleton<IPCSCDeviceService, PCSCDeviceService>();

        })
        .ConfigureLogging(logBuilder =>
        {
            logBuilder.SetMinimumLevel(LogLevel.Information);
            logBuilder.ClearProviders();
            logBuilder.AddNLog("NLog.config");
        }).Build();


    /// <summary>
    /// Gets services.
    /// </summary>
    public static IServiceProvider Services
    {
        get { return _host.Services; }
    }

    /// <summary>
    /// Occurs when the application is loading.
    /// </summary>
    private async void OnStartup(object sender, StartupEventArgs e)
    {
        await _host.StartAsync();
    }

    /// <summary>
    /// Occurs when the application is closing.
    /// </summary>
    private async void OnExit(object sender, ExitEventArgs e)
    {
        await _host.StopAsync();

        _host.Dispose();
    }

    /// <summary>
    /// Occurs when an exception is thrown by an application but not handled.
    /// </summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
    }
}
