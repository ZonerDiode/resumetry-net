using Resumetry.Domain.Enums;

namespace Resumetry.Application.Services
{
    /// <summary>
    /// Provides functionality to manage and determine valid status transitions for job applications based on predefined
    /// workflow rules.
    /// </summary>
    public class StatusStateEngine
    {
        private readonly static StatusEnum[] _interviewStatuses = [StatusEnum.Offer, StatusEnum.NoOffer, StatusEnum.Withdrawn];
        private readonly static StatusEnum[] _screenStatuses = [StatusEnum.Interview];
        private readonly static StatusEnum[] _appliedStatuses = [StatusEnum.Rejected, StatusEnum.Screen];

        /// <summary>
        /// Expected flow of a job application:
        /// Applied -> Rejected, Screen
        /// Screen -> Interview
        /// Interview -> Offer, NoOffer, Withdrawn
        /// </summary>
        /// <param name="statuses">Collection of current statuses used on a Job Application</param>
        /// <returns>Array of allowed status transitions based on the expected flow of a job application</returns>
        public static StatusEnum[] AvailableStatuses(ICollection<StatusEnum> statuses)
        {
            if (statuses is null || statuses.Count == 0)
            {
                return [StatusEnum.Applied];
            }

            // Allowed status transitions based on the expected flow of a job application
            if (statuses.Contains(StatusEnum.Interview) && !statuses.Intersect(_interviewStatuses).Any())
            {
                return [.. _interviewStatuses];
            }
            else if (statuses.Contains(StatusEnum.Screen) && !statuses.Intersect(_screenStatuses).Any())
            {
                return [.. _screenStatuses];
            }
            else if (statuses.Contains(StatusEnum.Applied) && !statuses.Intersect(_appliedStatuses).Any())
            {
                return [.. _appliedStatuses];
            }
            else
            {
                return [];
            }
        }
    }
}
