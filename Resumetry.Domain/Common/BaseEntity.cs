using System;
using System.Collections.Generic;
using System.Text;

namespace Resumetry.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
