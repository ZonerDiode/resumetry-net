using Resumetry.Domain.Common;

namespace Resumetry.Domain.Entities
{
    public class Recruiter : BaseEntity
    {
        public required string Name { get; set; }
        public string? Company { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}