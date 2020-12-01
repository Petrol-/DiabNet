using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DiabNet.Domain;

namespace DiabNet.Features.Synchronization.Nightscout
{
    public class NightscoutApi
    {
        private const string ClientName = "nightscout";
        private readonly IHttpClientFactory _clientFactory;

        public NightscoutApi(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private HttpClient GetClient() => _clientFactory.CreateClient(ClientName);

        public async Task<IList<Sgv>> GetEntries()
        {
            var response = await GetClient().GetAsync("entries.json");
            if (!response.IsSuccessStatusCode)
                throw new NightscoutException();
            var values =
                await JsonSerializer.DeserializeAsync<List<SgvDto>>(await response.Content.ReadAsStreamAsync());
            return values?.Select(v => new Sgv
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
