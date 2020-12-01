using System;

namespace DiabNet.Features.Search.Models
{
    public class SgvPoint
    {
        public SgvPoint(string id)
        {
            Id = id;
        }
        public string Id { get; }

        public DateTimeOffset Date { get; set; }

        public double Value { get; set; }

        public Trend Trend { get; set; } = Trend.Unknown;

        public double Delta { get; set; }
    }
}
