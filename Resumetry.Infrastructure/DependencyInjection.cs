using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resumetry.Application.Enums;
using Resumetry.Application.Interfaces;
using Resumetry.Application.Services;
using Resumetry.Domain.Interfaces;
using Resumetry.Infrastructure.Data;
using Resumetry.Infrastructure.Data.Repositories;
using Resumetry.Infrastructure.Services;

namespace Resumetry.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "Resumetry");
        Directory.CreateDirectory(appFolder);
        var dbPath = Path.Combine(appFolder, "resumetry.db");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IFileService, FileService>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        // Ensure database is created
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}
