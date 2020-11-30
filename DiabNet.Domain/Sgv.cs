using System;

namespace DiabNet.Domain
{
    public class Sgv
    {
        public string Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public double Value { get; set; }
        public SgvTrend Trend;
        public double Delta { get; set; }
    }
}