using Resumetry.Application.DTOs;
using System.Collections.Immutable;

namespace Resumetry.Application.Interfaces
{
    public interface ISankeyReportService
    {
        /// <summary>
        /// Generates a Sankey report based on job application data.
        /// </summary>
        /// <returns>A dictionary containing the report data for each stage of the application process.</returns>
        Task<ImmutableList<SankeyReportData>> GenerateSankeyReport();
    }
}
