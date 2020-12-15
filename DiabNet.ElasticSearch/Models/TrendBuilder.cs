using System;
using DiabNet.Domain;

namespace DiabNet.ElasticSearch.Models
{
    public static class TrendBuilder
    {
        public static Trend From(SgvTrend trend)
        {
            return trend switch
            {
                SgvTrend.Flat => Trend.Flat,
                SgvTrend.FortyFiveUp => Trend.FortyFiveUp,
                SgvTrend.Up => Trend.Up,
                SgvTrend.DoubleUp => Trend.DoubleUp,
                SgvTrend.FortyFiveDown => Trend.FortyFiveDown,
                SgvTrend.Down => Trend.Down,
                SgvTrend.DoubleDown => Trend.DoubleDown,
                SgvTrend.Unknown => Trend.Unknown,
                _ => Trend.Unknown
            };
        }
    }
}
