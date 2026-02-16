using Resumetry.Application.DTOs;
using Resumetry.Domain.Enums;

namespace Resumetry.ViewModels
{
    public class JobApplicationViewModel(JobApplicationSummaryDto dto) : ViewModelBase
    {
        public Guid Id => dto.Id;
        public string Company => dto.Company;
        public string Position => dto.Position;
        public string? Salary => dto.Salary;
        public bool TopJob => dto.TopJob;
        public DateTime CreatedAt => dto.CreatedAt;
        public StatusEnum? CurrentStatus => dto.CurrentStatus;
        public string CurrentStatusText => dto.CurrentStatusText;
        public DateTime? AppliedDate => dto.AppliedDate;
        public RecruiterDto? Recruiter => dto.Recruiter;
        public List<ApplicationEventDto> ApplicationEvents => dto.ApplicationEvents;

        /// <summary>
        /// Display string for the recruiter name, defaults to "No Recruiter" when absent.
        /// </summary>
        public string RecruiterName => dto.Recruiter?.Name ?? "No Recruiter";

        /// <summary>
        /// Display string for the recruiter company, defaults to empty when absent.
        /// </summary>
        public string RecruiterCompany => dto.Recruiter?.Company ?? string.Empty;
    }
}
