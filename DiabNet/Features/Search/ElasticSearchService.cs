using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiabNet.Features.Search.Models;
using Nest;

namespace DiabNet.Features.Search
{
    public class ElasticSearchService : ISearchService
    {
        private readonly ElasticClient _client;
        public const string EntryIndex = "entries";
        public const string MetaIndex = "metas";


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

            var metaIndex = await _client.Indices.ExistsAsync(MetaIndex);
            if (!metaIndex.Exists)
            {
                var descriptor = new CreateIndexDescriptor(MetaIndex)
                    .Map<MetaPoint>(m => m
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
                throw new Exception("could not index document", result.OriginalException);
            }
        }

        public async Task InsertMetaPoint(MetaPoint point)
        {
            var result = await _client
                .IndexAsync(point, (s) => s
                    .Index(MetaIndex));
            if (!result.IsValid)
            {
                throw new Exception("could not index document", result.OriginalException);
            }
        }

        public async Task<IEnumerable<SgvPoint>> FetchSgvPointRange(DateTimeOffset from, DateTimeOffset to, int? limit = null)
        {
            var result = await _client.SearchAsync<SgvPoint>(s => s
                .Index(EntryIndex)
                .Size(limit)
                .Query(q => q
                    .DateRange(d => d
                        .Field(field => field.Date)
                        .GreaterThanOrEquals(from.DateTime)
                        .LessThanOrEquals(to.DateTime)))
                .Sort(sort => sort.Descending(desc => desc.Date)));
           return  result.Documents;
        }
    }
}
