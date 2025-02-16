// 参考URL
// https://zenn.dev/microsoft/articles/wpf-generic-host
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using System.Windows;

using MinatoProject.PCSCSampleWpf;
using MinatoProject.PCSCSampleWpf.Views.Windows;
using MinatoProject.PCSCSampleWpf.ViewModels;
using Microsoft.Extensions.Logging;

Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<WpfBootstrapper>(); // 下で定義しているクラス

// App と MainWindow を登録
builder.Services.AddSingleton<App>();
builder.Services.AddTransient<MainWindow>();
builder.Services.AddTransient<MainWindowViewModel>();

// ロガーの登録
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.ClearProviders();
builder.Logging.AddNLog("NLog.config");

var app = builder.Build();
app.Run();

// WPF の起動をする
class WpfBootstrapper(
    App app,
    MainWindow mainWindow,
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        app.Startup += App_Startup;
        app.Exit += App_Exit;
        app.Run();
        return Task.CompletedTask;
    }
    private void App_Startup(object sender, StartupEventArgs e)
    {
        mainWindow.Show();
    }
    private void App_Exit(object sender, ExitEventArgs e)
    {
        hostApplicationLifetime.StopApplication();
    }
}
