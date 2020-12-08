using System;
using System.Collections.Generic;
using DiabNet.Features.Search.Models;
using Nest;

namespace DiabNet.ElasticSearch.Models
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

        public IList<string> Tags { get; set; } = new List<string>();

        public Treatment? Treatment { get; set; }
    }
}
