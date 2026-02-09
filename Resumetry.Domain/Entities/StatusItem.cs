using Resumetry.Domain.Common;
using Resumetry.Domain.Enums;

namespace Resumetry.Domain.Entities
{
    public class StatusItem : BaseEntity
    {
        public required DateTime Occurred { get; set; }
        public required StatusEnum Status { get; set; }
    }
}