using Resumetry.Application.DTOs;
using Resumetry.Domain.Enums;

namespace Resumetry.ViewModels
{
    public class JobApplicationViewModel : ViewModelBase
    {
        private readonly JobApplicationSummaryDto _dto;

        public JobApplicationViewModel(JobApplicationSummaryDto dto)
        {
            _dto = dto;
        }

        public Guid Id => _dto.Id;
        public string Company => _dto.Company;
        public string Position => _dto.Position;
        public string? Salary => _dto.Salary;
        public bool TopJob => _dto.TopJob;
        public DateTime CreatedAt => _dto.CreatedAt;
        public StatusEnum? CurrentStatus => _dto.CurrentStatus;
        public string CurrentStatusText => _dto.CurrentStatusText;
        public DateTime? AppliedDate => _dto.AppliedDate;
    }
}
