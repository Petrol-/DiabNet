using System;
using System.Threading.Tasks;
using DiabNet.Domain;
using DiabNet.Features.Search;
using DiabNet.Features.Search.Models;
using Nest;
using NUnit.Framework;

namespace DiabNet.TestIT.Features.Search
{
    public class ElasticSearchServiceTestIt
    {
        private ElasticSearchService _searchService;
        private ElasticClient _client;

        [SetUp]
        public async Task Setup()
        {
            _client = new ElasticClient(new Uri(ElasticSearchTestContainer.Url));
            _searchService = new ElasticSearchService(_client);

            //Remove the index (clear the data)
            await _client.Indices.DeleteAsync(ElasticSearchService.EntryIndex);
            //Create indices to allow all tests to work
            await _searchService.EnsureInitialized();
        }

        [Test]
        public async Task Initialize_should_ensure_entry_index_is_created()
        {
            //For this test, delete Index, it should be recreated
            await _client.Indices.DeleteAsync(ElasticSearchService.EntryIndex);

            await _searchService.EnsureInitialized();
            var existsResponse = await _client.Indices.ExistsAsync(ElasticSearchService.EntryIndex);
            Assert.IsTrue(existsResponse.Exists);
        }

        [Test]
        public async Task Should_insert_sgv()
        {
            var toInsert = new SgvPoint(DateTimeOffset.UtcNow.ToString())
            {
                Date = DateTimeOffset.Now,
                Delta = 1,
                Trend = Trend.Flat,
                Value = new Random().Next()
            };
            await _searchService.InsertSgvPoint(toInsert);

            var response = await _client.GetAsync<Sgv>(toInsert.Id, i => i.Index(ElasticSearchService.EntryIndex));
            Assert.IsTrue(response.Found);
            Assert.AreEqual(toInsert.Value, response.Source.Value);
        }
    }
}
