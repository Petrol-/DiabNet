using System;
using System.Collections.Generic;
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
            await _client.Indices.DeleteAsync(ElasticSearchService.MetaIndex);
            //Create indices to allow all tests to work
            await _searchService.EnsureInitialized();
        }

        #region Initialize

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
        public async Task Initialize_should_ensure_meta_index_is_created()
        {
            //For this test, delete Index, it should be recreated
            await _client.Indices.DeleteAsync(ElasticSearchService.MetaIndex);

            await _searchService.EnsureInitialized();
            var existsResponse = await _client.Indices.ExistsAsync(ElasticSearchService.MetaIndex);
            Assert.IsTrue(existsResponse.Exists);
        }

        #endregion

        #region Entry

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

            var response = await _client.GetAsync<SgvPoint>(toInsert.Id,
                i => i.Index(ElasticSearchService.EntryIndex));
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
            await Task.Delay(1000); //give a little time to ingest data

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

        #endregion

        #region Metas

        [Test]
        public async Task Should_insert_meta_point()
        {
            var toInsert = new MetaPoint(DateTimeOffset.UtcNow.ToString())
            {
                Date = DateTimeOffset.Now,
                Delta = 1,
                Trend = Trend.Flat,
                Value = new Random().Next(), Tags = new List<string> {"hello", "world"},
                Treatment = new Treatment {FastInsulin = 1, SlowInsulin = 2}
            };
            await _searchService.InsertMetaPoint(toInsert);

            var response = await _client.GetAsync<MetaPoint>(toInsert.Id, i => i.Index(ElasticSearchService.MetaIndex));
            Assert.IsTrue(response.Found);
            Assert.AreEqual(toInsert.Value, response.Source.Value);
        }

        private async Task InsertMetaPointAtDate(DateTimeOffset date)
        {
            var toInsert = new MetaPoint(date.ToString())
            {
                Date = date,
                Delta = 1,
                Trend = Trend.Flat,
                Value = new Random().Next(),
                Tags = new List<string> {"hello", "world"},
                Treatment = new Treatment {FastInsulin = 1, SlowInsulin = 2}
            };
            await _searchService.InsertMetaPoint(toInsert);
        }

        #endregion
    }
}
