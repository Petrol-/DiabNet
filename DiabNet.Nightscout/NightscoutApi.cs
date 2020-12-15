using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DiabNet.Domain;

namespace DiabNet.Nightscout
{
    public class NightscoutApi : INightscoutApi
    {
        private readonly HttpClient _client;

        public NightscoutApi(HttpClient client)
        {
            _client = client;
        }


        public async Task<IList<Sgv>> GetEntries(DateTimeOffset from, DateTimeOffset to)
        {
            var endpoint =
                $"entries.json?find[dateString][$gte]={from.ToStartOfDay():s}&find[dateString][$lte]={to.ToEndDay():s}&count=10000";

            var response = await _client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
                throw new NightscoutException();
            var values =
                await JsonSerializer.DeserializeAsync<List<SgvDto>>(await response.Content.ReadAsStreamAsync());

            if (values == null) throw new NightscoutException();
            return values.Select(v => new Sgv
            {
                Id = v.Id,
                Date = v.Date,
                Delta = v.Delta,
                Value = v.Sgv,
                Source = v.Device,
                Trend = v.ToTrend()
            }).ToList();
        }
    }
}
