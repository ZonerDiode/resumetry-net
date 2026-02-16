using Resumetry.Domain.Enums;
using Resumetry.Domain.Interfaces;
using Resumetry.Application.DTOs;
using System.Collections.Immutable;
using Resumetry.Application.Interfaces;

namespace Resumetry.Application.Services
{
    /// <summary>
    /// Creates a Sankey report based on job application data.
    /// </summary>
    /// <param name="unitOfWork">Unit of work provider.</param>
    public class SankeyReportService(IUnitOfWork unitOfWork) : ISankeyReportService
    {
        private readonly string APPLIED_NORESPONSE = "APPLIED->NO RESPONSE";
        private readonly string APPLIED_RESPONDED = "APPLIED->RESPONDED";
        private readonly string RESPONDED_REJECTED = "RESPONDED->REJECTED";
        private readonly string RESPONDED_INTERVIEW = "RESPONDED->INTERVIEW";
        private readonly string INTERVIEW_OFFER = "INTERVIEW->OFFER";
        private readonly string INTERVIEW_NOOFFER = "INTERVIEW->NO OFFER";

        private readonly ImmutableList<StatusEnum> respondedStatuses =
        [
            StatusEnum.Rejected,
            StatusEnum.Screen,
            StatusEnum.Interview
        ];

        private readonly ImmutableList<StatusEnum> interviewStatuses =
        [
            StatusEnum.Screen,
            StatusEnum.Interview
        ];

        /// <summary>
        /// Generates a Sankey report that categorizes job applications based on their status progression.
        /// </summary>
        /// <returns>A task result containing an immutable list of
        /// SankeyReportData objects, each representing a category of job application outcomes sorted by count in
        /// descending order.</returns>
        public async Task<ImmutableList<SankeyReportData>> GenerateSankeyReport()
        {
            var jobs = await unitOfWork.JobApplications.GetAllAsync();
            var reportData = InitializeReportData();

            foreach (var job in jobs)
            {
                if (job.ApplicationStatuses.Count == 0)
                {
                    continue;
                }

                // If there's only one status, it means the user applied but hasn't received any response yet
                if (job.ApplicationStatuses.Count == 1)
                {
                    reportData[APPLIED_NORESPONSE].Increment();
                    continue;
                }

                // If there are any response statuses, it means the user has received some response
                if (job.ApplicationStatuses.Any(a => respondedStatuses.Contains(a.Status)))
                {
                    reportData[APPLIED_RESPONDED].Increment();
                }

                // If there's any rejected status, it means the user received a rejection
                if (job.ApplicationStatuses.Any(a => a.Status == StatusEnum.Rejected))
                {
                    reportData[RESPONDED_REJECTED].Increment();
                    continue;
                }

                // If there's any interview status, it means the user received an interview invitation
                if (job.ApplicationStatuses.Any(a => interviewStatuses.Contains(a.Status)))
                {
                    reportData[RESPONDED_INTERVIEW].Increment();
                }

                // Ends with either an offer or no offer
                if (job.ApplicationStatuses.Any(a => a.Status == StatusEnum.Offer))
                {
                    reportData[INTERVIEW_OFFER].Increment();
                }
                else
                {
                    reportData[INTERVIEW_NOOFFER].Increment();
                }
            }

            // Return List sorted by count in descending order
            return [.. reportData.OrderByDescending(kv => kv.Value.Count).Select(kv => kv.Value)];
        }

        private Dictionary<string, SankeyReportData> InitializeReportData()
        {
            return new Dictionary<string, SankeyReportData>
            {
                [APPLIED_NORESPONSE] = new SankeyReportData("Applied", "No Response"),
                [APPLIED_RESPONDED] = new SankeyReportData("Applied", "Responded"),
                [RESPONDED_REJECTED] = new SankeyReportData("Responded", "Rejected"),
                [RESPONDED_INTERVIEW] = new SankeyReportData("Responded", "Interview"),
                [INTERVIEW_OFFER] = new SankeyReportData("Interview", "Offer"),
                [INTERVIEW_NOOFFER] = new SankeyReportData("Interview", "No Offer")
            };
        }
    }
}
