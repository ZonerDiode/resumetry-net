using Microsoft.Extensions.DependencyInjection;
using Resumetry.Application.Enums;
using Resumetry.Application.Interfaces;
using Resumetry.Application.Services;

namespace Resumetry.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddKeyedScoped<IImportService, ImportService>(ImportType.Standard);
            services.AddKeyedScoped<IImportService, LegacyImportService>(ImportType.Legacy);
            services.AddScoped<IExportService, ExportService>();
            services.AddScoped<IJobApplicationService, JobApplicationService>();
            services.AddScoped<ISankeyReportService, SankeyReportService>();

            return services;
        }
    }
}
