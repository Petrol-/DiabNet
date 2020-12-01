using System;
using System.Text.Json.Serialization;
using DiabNet.Domain;

namespace DiabNet.Features.Synchronization.Nightscout
{
    public class SgvDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("dateString")]
        public DateTimeOffset Date { get; set; }

        [JsonPropertyName("sgv")]
        public double Sgv { get; set; }
        
        [JsonPropertyName("direction")]
        public String Direction { get; set; }
        
        [JsonPropertyName("delta")]
        public double Delta { get; set; }


        public SgvTrend ToTrend()
        {
            switch (Direction)
            {
               case "Flat" : return SgvTrend.Flat;
               default : return SgvTrend.Unknown;
            }
        }
    }
}
