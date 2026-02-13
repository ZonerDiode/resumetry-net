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
    }
}
