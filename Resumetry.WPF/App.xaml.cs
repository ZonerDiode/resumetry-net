using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Resumetry.Application.Interfaces;
using Resumetry.Application.Services;
using Resumetry.Domain.Interfaces;
using Resumetry.Infrastructure.Data;
using Resumetry.Infrastructure.Data.Repositories;
using Resumetry.Infrastructure.Services;
using Resumetry.ViewModels;
using Resumetry.WPF.Services;
using System.IO;
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
                    ConfigureServices(services);
                })
                .Build();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Get database path in AppData
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "Resumetry");
            Directory.CreateDirectory(appFolder);
            var dbPath = Path.Combine(appFolder, "resumetry.db");

            // Register DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Register repositories and unit of work
            services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register services
            services.AddSingleton<IFileService, FileService>();
            services.AddScoped<IImportService, ImportService>();
            services.AddScoped<IJobApplicationService, JobApplicationService>();

            // Register WPF services
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IScopedRunner, ScopedRunner>();

            // Register ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<ApplicationFormViewModel>();
            services.AddTransient<SettingsViewModel>();

            // Register Windows
            services.AddTransient<MainWindow>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            // Ensure database is created
            using (var scope = _host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await dbContext.Database.MigrateAsync();
            }

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

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
