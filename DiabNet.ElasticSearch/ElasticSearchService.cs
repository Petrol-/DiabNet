using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiabNet.Domain.Services;
using DiabNet.ElasticSearch.Models;
using Nest;

namespace DiabNet.ElasticSearch
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
                throw new Exception("could not index document", result.OriginalException);
            }
        }

        public async Task<IEnumerable<SgvPoint>> FetchSgvPointRange(DateTimeOffset from, DateTimeOffset to,
            int? limit = null)
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
            return result.Documents;
        }

        public async Task<IEnumerable<SgvPoint>> SearchSimilarPoints(SgvPoint point, int limit = 10)
        {
            var search = await _client.SearchAsync<SgvPoint>(s => s
                .Index(EntryIndex)
                .Size(limit)
                .Query(q => q
                    .FunctionScore(fs => fs
                        .Query(fsq => fsq
                            .Range(r => r
                                .Field(f => f.Value)
                                .GreaterThanOrEquals(point.Value / 2)
                                .LessThanOrEquals(point.Value * 2)
                            )
                        )
                        .ScoreMode(FunctionScoreMode.Sum)
                        .Functions(f => f
                            .Linear(b =>
                            {
                                return b.Field(p => p.Trend)
                                    .Offset(0)
                                    .Scale(10)
                                    .Origin((int) point.Trend)
                                    .Weight(8);
                            })
                            .Gauss(b => b.Field(p => p.Value)
                                .Offset(point.Value * 0.1)
                                .Scale(point.Value * 0.5)
                                .Decay(0.2)
                                .Origin(point.Value)
                                .Weight(10))
                            .ScriptScore(b => b
                                .Weight(1)
                                .Script(sc => sc.Source(@"
                                     int score = 0;
                                     def tags = params['tags'];
                                     def mesureTags = doc['tags.keyword'];
                                     for(def i =0; i<tags.length;i++)
                                     {
                                         if(mesureTags.contains(tags[i])){
                                         score++;
                                         }
                                     }
                                     return score;
                                     ")
                                    .Params(p => p.Add("tags", point.Tags)))
                            )
                        ))));
            return search.Documents;
        }
    }
}
