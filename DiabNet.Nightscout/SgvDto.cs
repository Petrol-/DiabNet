using System;
using System.Text.Json.Serialization;
using DiabNet.Domain;

namespace DiabNet.Nightscout
{
    public class SgvDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; }

        [JsonPropertyName("dateString")]
        public DateTimeOffset Date { get; }

        [JsonPropertyName("sgv")]
        public double Sgv { get;  }
        
        [JsonPropertyName("direction")]
        public string Direction { get; }
        
        [JsonPropertyName("delta")]
        public double Delta { get; }


        public SgvTrend ToTrend()
        {
            return Direction switch
            {
                "Flat" => SgvTrend.Flat,
                _ => SgvTrend.Unknown
            };
        }
    }
}
