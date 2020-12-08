using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DiabNet.Domain;

namespace DiabNet.Nightscout
{
    public class NightscoutApi
    {
        private readonly HttpClient _client;

        public NightscoutApi(HttpClient client)
        {
            _client = client;
        }


        public async Task<IList<Sgv>> GetEntries()
        {
            var response = await _client.GetAsync("entries.json");
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
                Trend = v.ToTrend()
            }).ToList();
        }
    }
}
