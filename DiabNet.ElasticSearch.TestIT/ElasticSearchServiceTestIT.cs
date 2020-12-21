using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiabNet.Domain;
using DiabNet.ElasticSearch.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Nest;
using NUnit.Framework;
using Treatment = DiabNet.Domain.Treatment;

namespace DiabNet.ElasticSearch.TestIT
{
    public class ElasticSearchServiceTestIt
    {
        private ElasticSearchService _searchService;
        private ElasticClient _client;
        private Mock<ILogger<ElasticSearchService>> _loggerMock;

        [SetUp]
        public async Task Setup()
        {
            _client = new ElasticClient(new Uri(ElasticSearchTestContainer.Url));
            _loggerMock = new Mock<ILogger<ElasticSearchService>>();
            _searchService = new ElasticSearchService(_client, _loggerMock.Object);

            //Remove the index (clear the data)
            await _client.Indices.DeleteAsync(ElasticSearchService.EntryIndex);
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

        #endregion

        #region Entry

        [Test]
        public async Task Should_insert_sgv()
        {
            var toInsert = new Sgv()
            {
                Id = DateTimeOffset.UtcNow.ToString(),
                Date = DateTimeOffset.Now,
                Delta = 1,
                Trend = SgvTrend.Flat,
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
            var toInsert = new Sgv()
            {
                Id = date.ToString(),
                Date = date,
                Delta = 1,
                Trend = SgvTrend.Flat,
                Value = new Random().Next()
            };
            await _searchService.InsertSgvPoint(toInsert);
        }

        [Test]
        public async Task SearchSimilarPoints_should_filter_result_with_value_2_times_given_value()
        {
            await InsertSvgPoint(SgvTrend.Unknown, 49, null);

            await InsertSvgPoint(SgvTrend.Unknown, 50, null);
            await InsertSvgPoint(SgvTrend.Unknown, 100, null);
            await InsertSvgPoint(SgvTrend.Unknown, 200, null);

            await InsertSvgPoint(SgvTrend.Unknown, 201, null);
            await Task.Delay(1000);

            var results = await _searchService.SearchSimilarPoints(new SgvPoint("")
            {
                Value = 100
            });

            Assert.AreEqual(3, results.Count());
        }

        [Test]
        public async Task SearchSimilarPoints_scoring_should_allow_null_treatment()
        {
            await InsertSvgPoint(SgvTrend.Unknown, 100, null);

            await Task.Delay(1000);
            var resuls = await _searchService.SearchSimilarPoints(new SgvPoint("test")
            {
                Value = 100
            });

            Assert.AreEqual(resuls.First().Value, 100);
        }

        [Test]
        public async Task SearchSimilarPoints_scoring_should_order_results_by_nearest_value()
        {
            await InsertSvgPoint(SgvTrend.Unknown, 100, null);
            await InsertSvgPoint(SgvTrend.Unknown, 120, null);
            await InsertSvgPoint(SgvTrend.Unknown, 130, null);

            await Task.Delay(1000);
            var results = await _searchService.SearchSimilarPoints(new SgvPoint("test")
            {
                Value = 100
            });
            var matches  = results.ToList();
            Assert.AreEqual(matches[0].Value, 100);
            Assert.AreEqual(matches[1].Value, 120);
            Assert.AreEqual(matches[2].Value, 130);
        }

        [Test]
        public async Task SearchSimilarPoints_scoring_should_order_results_by_nearest_trend()
        {
            await InsertSvgPoint(SgvTrend.DoubleDown, 100, null);
            await InsertSvgPoint(SgvTrend.Down, 100, null);
            await InsertSvgPoint(SgvTrend.Flat, 100, null);

            await Task.Delay(1000);
            var results = await _searchService.SearchSimilarPoints(new SgvPoint("test")
            {
                Value = 100,
                Trend = Trend.Flat
            });
            var matches  = results.ToList();
            Assert.AreEqual(matches[0].Trend, Trend.Flat);
            Assert.AreEqual(matches[1].Trend, Trend.Down);
            Assert.AreEqual(matches[2].Trend, Trend.DoubleDown);
        }

        [Test]
        public async Task SearchSimilarPoints_scoring_should_order_results_by_number_of_matching_tags()
        {
            await InsertSvgPoint(SgvTrend.Unknown, 100, null);
            await InsertSvgPoint(SgvTrend.Unknown, 101, null, "coca");
            await InsertSvgPoint(SgvTrend.Unknown, 102, null,"coca", "pain");

            await Task.Delay(1000);
            var results = await _searchService.SearchSimilarPoints(new SgvPoint("test")
            {
                Value = 100,
                Tags = new List<string>{"coca", "pain"}
            });
            var matches  = results.ToList();
            Assert.AreEqual(matches[0].Tags.Count, 2);
            Assert.AreEqual(matches[1].Tags.Count, 1);
            Assert.AreEqual(matches[2].Tags.Count, 0);
        }

        private async Task InsertSvgPoint(SgvTrend trend, double value, Treatment treatment, params string[] tags)
        {
            Sgv toInsert = new()
            {
                Id = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
                Date = DateTimeOffset.Now,
                Trend = trend,
                Value = value,
                Tags = tags,
                Treatment = treatment
            };
            await _searchService.InsertSgvPoint(toInsert);
        }

        #endregion
    }
}
