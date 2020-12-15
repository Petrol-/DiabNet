using System;
using System.Collections.Generic;

namespace DiabNet.Domain
{
    public class Sgv
    {
        public string Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public double Value { get; set; }
        public SgvTrend Trend;
        public double Delta { get; set; }

        public IList<string> Tags { get; set; } = new List<string>();

        public Treatment? Treatment { get; set; }

        public string Source { get; set; }
    }
}
