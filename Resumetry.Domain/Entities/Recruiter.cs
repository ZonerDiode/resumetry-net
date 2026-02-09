using Resumetry.Domain.Common;

namespace Resumetry.Domain.Entities
{
    public class Recruiter : BaseEntity
    {
        public required string Name { get; init; }
        public string? Company { get; init; }
        public string? Email { get; init; }
        public string? Phone { get; init; }
    }
}