using System.Threading.Tasks;
using Nest;

namespace DiabNet.Features.Search
{
    public class ElasticSearchService : ISearchService
    {
        private readonly ElasticClient _client;
        public const string EntryIndex = "entries";


        public ElasticSearchService(ElasticClient client)
        {
            _client = client;
        }

        public async Task EnsureInitialized()
        {
            // var entryIndex = await _client.Indices.ExistsAsync(EntryIndex);
            // if (!entryIndex.Exists)
            // {
            // //    _client.Indices.Create()
            // }
        }
    }
}
