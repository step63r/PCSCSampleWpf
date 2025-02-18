using Microsoft.Extensions.Hosting;
using MinatoProject.PCSCSampleWpf.Views.Windows;
using System.Windows;
using Wpf.Ui;

namespace MinatoProject.PCSCSampleWpf.Services
{
    /// <summary>
    /// Managed host of the application.
    /// </summary>
    /// <param name="serviceProvider"></param>
    public class ApplicationHostService(IServiceProvider serviceProvider) : IHostedService
    {
        /// <summary>
        /// 
        /// </summary>
        private INavigationWindow? _navigationWindow;

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await HandleActivationAsync();
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates main window during activation.
        /// </summary>
        private async Task HandleActivationAsync()
        {
            await Task.CompletedTask;

            if (!Application.Current.Windows.OfType<MainWindow>().Any())
            {
                _navigationWindow = (serviceProvider.GetService(typeof(INavigationWindow)) as INavigationWindow)!;
                _navigationWindow!.ShowWindow();

                _ = _navigationWindow.Navigate(typeof(Views.Pages.ReaderPage));
            }

            await Task.CompletedTask;
        }
    }
}
