using System;
using System.Linq;
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

        [Test]
        public async Task Should_return_point_between_from_and_to_dates()
        {
            var from = DateTimeOffset.Now;
            var to = DateTimeOffset.Now.AddHours(3);
            var point1 = from.AddHours(1);
            var point2 = from.AddHours(2);
            var point3 = from.AddDays(10);
            await InsertSgvPointAtDate(point1);
            await InsertSgvPointAtDate(point2);
            await InsertSgvPointAtDate(point3);
            await Task.Delay(500); //give a little time to ingest data

            var results = await _searchService.FetchSgvPointRange(from, to);

            Assert.That(results.Count(), Is.EqualTo(2));
        }

        private async Task InsertSgvPointAtDate(DateTimeOffset date)
        {
            var toInsert = new SgvPoint(date.ToString())
            {
                Date = date,
                Delta = 1,
                Trend = Trend.Flat,
                Value = new Random().Next()
            };
            await _searchService.InsertSgvPoint(toInsert);
        }
    }
}
