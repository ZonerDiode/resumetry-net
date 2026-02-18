using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Resumetry.ViewModels;
using Resumetry.WPF.Services;
using Resumetry.Infrastructure;
using Resumetry.Application;
using System.Windows;

namespace Resumetry
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services, context.Configuration);
                })
                .Build();
        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddInfrastructure();
            services.AddApplication();

            // Register WPF services
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IScopedRunner, ScopedRunner>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Register ViewModels
            services.AddSingleton<ShellViewModel>();
            services.AddTransient<JobApplicationListViewModel>();
            services.AddTransient<ApplicationFormViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SankeyReportViewModel>();

            // Register Windows
            services.AddTransient<MainWindow>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            await _host.Services.InitializeDatabaseAsync();

            // Initialize navigation service with shell view model
            var navigationService = (NavigationService)_host.Services.GetRequiredService<INavigationService>();
            var shellViewModel = _host.Services.GetRequiredService<ShellViewModel>();
            navigationService.Initialize(shellViewModel);

            // Register view mappings
            navigationService.RegisterView<JobApplicationListViewModel, Views.JobApplicationListView>();
            navigationService.RegisterView<ApplicationFormViewModel, Views.ApplicationFormView>();
            navigationService.RegisterView<SettingsViewModel, Views.SettingsView>();
            navigationService.RegisterView<SankeyReportViewModel, Views.SankeyReportView>();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            // Navigate to home view
            navigationService.NavigateToHome();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync();
            }

            base.OnExit(e);
        }
    }
}
