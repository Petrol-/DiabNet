using System;
using System.Threading.Tasks;
using DiabNet.Features.Search.Models;
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
            var entryIndex = await _client.Indices.ExistsAsync(EntryIndex);
            if (!entryIndex.Exists)
            {
                var descriptor = new CreateIndexDescriptor(EntryIndex)
                    .Map<SgvPoint>(m => m
                        .AutoMap());
                await _client.Indices.CreateAsync(descriptor);
            }
        }

        public async Task InsertSgvPoint(SgvPoint sgv)
        {
            var result = await _client
                .IndexAsync(sgv, (s) => s
                    .Index(EntryIndex));
            if (!result.IsValid)
            {
                throw new Exception("could not index document");
            }
        }
    }
}
