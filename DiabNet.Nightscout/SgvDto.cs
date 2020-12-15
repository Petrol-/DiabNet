using System;
using System.Text.Json.Serialization;
using DiabNet.Domain;

namespace DiabNet.Nightscout
{
    public class SgvDto
    {
        [JsonPropertyName("_id")] public string Id { get; set; }

        [JsonPropertyName("dateString")] public DateTimeOffset Date { get; set; }

        [JsonPropertyName("sgv")] public double Sgv { get; set; }

        [JsonPropertyName("direction")] public string Direction { get; set; }

        [JsonPropertyName("device")] public string Device { get; set; }

        [JsonPropertyName("delta")] public double Delta { get; set; }


        public SgvTrend ToTrend()
        {
            return Direction switch
            {
                "Flat" => SgvTrend.Flat,
                "Up" => SgvTrend.Up,
                "Down" => SgvTrend.Down,
                "FortyFiveUp" => SgvTrend.FortyFiveUp,
                "FortyFiveDown" => SgvTrend.FortyFiveDown,
                "DoubleUp" => SgvTrend.DoubleUp,
                "DoubleDown" => SgvTrend.DoubleDown,
                
                _ => SgvTrend.Unknown
            };
        }
    }
}
