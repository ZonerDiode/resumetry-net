using Resumetry.Domain.Common;

namespace Resumetry.Domain.Entities
{
    public class ApplicationEvent : BaseEntity
    {
        public required DateTime Date { get; init; }
        public required string Description { get; init; }
    }
}