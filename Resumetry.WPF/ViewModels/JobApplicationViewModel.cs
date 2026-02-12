using Resumetry.Domain.Entities;
using Resumetry.Domain.Enums;

namespace Resumetry.ViewModels
{
    public class JobApplicationViewModel : ViewModelBase
    {
        private readonly JobApplication _jobApplication;

        public JobApplicationViewModel(JobApplication jobApplication)
        {
            _jobApplication = jobApplication;
        }

        public Guid Id => _jobApplication.Id;
        public string Company => _jobApplication.Company;
        public string Position => _jobApplication.Position;
        public string? Salary => _jobApplication.Salary;
        public bool TopJob => _jobApplication.TopJob;
        public DateTime CreatedAt => _jobApplication.CreatedAt;

        public StatusEnum? CurrentStatus
        {
            get
            {
                var latestStatus = _jobApplication.StatusItems
                    .OrderByDescending(s => s.Occurred)
                    .FirstOrDefault();
                return latestStatus?.Status;
            }
        }

        public string CurrentStatusText => CurrentStatus?.ToString() ?? "UNKNOWN";

        public DateTime? AppliedDate
        {
            get
            {
                var appliedStatus = _jobApplication.StatusItems
                    .Where(s => s.Status == StatusEnum.APPLIED)
                    .OrderBy(s => s.Occurred)
                    .FirstOrDefault();
                return appliedStatus?.Occurred ?? _jobApplication.CreatedAt;
            }
        }
    }
}
