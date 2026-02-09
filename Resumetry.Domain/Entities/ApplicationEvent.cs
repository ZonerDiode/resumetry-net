using Resumetry.Domain.Common;

namespace Resumetry.Domain.Entities
{
    public class ApplicationEvent : BaseEntity
    {
        public required DateTime Occurred { get; set; }
        public required string Description { get; set; }
    }
}