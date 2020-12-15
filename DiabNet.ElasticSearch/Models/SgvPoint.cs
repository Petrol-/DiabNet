using System;
using System.Collections.Generic;
using DiabNet.Domain;
using Nest;

namespace DiabNet.ElasticSearch.Models
{
    [ElasticsearchType(IdProperty = nameof(Id))]
    public class SgvPoint
    {
        public static SgvPoint From(Sgv sgv)
        {
            return new(sgv.Id)
            {
                Date = sgv.Date,
                Delta = sgv.Delta,
                Tags = sgv.Tags,
                Treatment = sgv.Treatment == null
                    ? null
                    : new Treatment
                    {
                        SlowInsulin = sgv.Treatment.SlowInsulin,
                        FastInsulin = sgv.Treatment.FastInsulin
                    },
                Trend = TrendBuilder.From(sgv.Trend),
                Value = sgv.Value,
                Source = sgv.Source
            };
        }

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

        public Treatment Treatment { get; set; }

        [Text(Analyzer = "not_analyzed")]
        public string Source { get; set; }
    }
}
