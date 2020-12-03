using System;
using Nest;

namespace DiabNet.Features.Search.Models
{
    [ElasticsearchType(IdProperty = nameof(Id))]

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
