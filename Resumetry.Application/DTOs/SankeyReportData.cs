using System;
using System.Collections.Generic;
using System.Text;

namespace Resumetry.Application.DTOs
{
    public record SankeyReportData(string From, string To)
    {
        public int Count { get; internal set; }

        internal void Increment() => Count++;
    }
}
