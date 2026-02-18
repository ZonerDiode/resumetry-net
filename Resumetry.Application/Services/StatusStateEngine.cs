using Resumetry.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resumetry.Application.Services
{
    /// <summary>
    /// Provides functionality to manage and determine valid status transitions for job applications based on predefined
    /// workflow rules.
    /// </summary>
    public class StatusStateEngine
    {
        /// <summary>
        /// Expected flow of a job application:
        /// Applied -> Rejected, Screen
        /// Screen -> Interview
        /// Interview -> Offer, NoOffer, Withdrawn
        /// </summary>
        /// <param name="currentStatuses">Collection of current statuses used on a Job Application</param>
        /// <returns>Array of allowed status transitions based on the expected flow of a job application</returns>
        public static StatusEnum[] AvailableStatuses(ICollection<StatusEnum> currentStatuses)
        {
            if (currentStatuses is null || currentStatuses.Count == 0)
            {
                return [StatusEnum.Applied];
            }

            // Allowed status transitions based on the expected flow of a job application
            if (currentStatuses.Contains(StatusEnum.Interview))
            {
                return [StatusEnum.Offer, StatusEnum.NoOffer, StatusEnum.Withdrawn];
            }
            else if (currentStatuses.Contains(StatusEnum.Screen))
            {
                return [StatusEnum.Interview];
            }
            else if (currentStatuses.Contains(StatusEnum.Applied))
            {
                return [StatusEnum.Rejected, StatusEnum.Screen];
            }
            else
            {
                return [];
            }
        }
    }
}
