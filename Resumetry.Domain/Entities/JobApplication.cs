using Resumetry.Domain.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Resumetry.Domain.Entities
{
    public class JobApplication : BaseEntity
    {
        public required string Company { get; set; }
        public required string Position { get; set; }
        public required string Description { get; set; }
        public required string Salary { get; set; }
        public bool TopJob { get; set; }
        public string? SourcePage { get; set; }
        public string? ReviewPage { get; set; }
        public string? LoginNotes { get; set; }
        public Recruiter? Recruiter { get; set; }
        public ICollection<ApplicationEvent> ApplicationEvents { get; set; } = [];
        public ICollection<StatusItem> StatusItems { get; set; } = [];
    }
}
