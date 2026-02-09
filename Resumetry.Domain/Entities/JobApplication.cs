using Resumetry.Domain.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Resumetry.Domain.Entities
{
    public class JobApplication : BaseEntity
    {
        public required string Company { get; init; }
        public required string Position { get; init; }
        public required string Description { get; init; }
        public required string Salary { get; init; }
        public bool TopJob { get; init; }
        public string? SourcePage { get; init; }
        public string? ReviewPage { get; init; }
        public string? LoginNotes { get; init; }
        public Recruiter? Recruiter { get; init; }
        public ICollection<ApplicationEvent> ApplicationEvents { get; init; } = [];
        public ICollection<StatusItem> StatusItems { get; init; } = [];
    }
}
