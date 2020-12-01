using System;
using System.Threading.Tasks;
using DiabNet.Domain;
using DiabNet.Features.Search;
using Microsoft.Extensions.Configuration;
using Nest;
using NUnit.Framework;

namespace DiabNet.TestIT.Features.Search
{
    public class ElasticSearchServiceTestIt
    {
        private string _esUrl;
        private ElasticSearchService _searchService;
        private ElasticClient _client;

        [OneTimeSetUp]
        public void Init()
        {
            var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            _esUrl = configuration["ELASTIC_URL"];
        }

        [SetUp]
        public void Setup()
        {
            _client = new ElasticClient(new Uri(_esUrl));
            _searchService = new ElasticSearchService(_client);
        }

        [Test]
        public async Task Initialize_should_ensure_entry_index_is_created()
        {
            await _searchService.EnsureInitialized();
            var existsResponse = await _client.Indices.ExistsAsync(ElasticSearchService.EntryIndex);
            Assert.IsTrue(existsResponse.Exists);
        }

        [Test]
        public async Task Should_insert_sgv()
        {
            var toInsert = new Sgv
            {
                Id = DateTimeOffset.UtcNow.ToString(),
                Date = DateTimeOffset.Now,
                Delta = 1,
                Trend = SgvTrend.Flat,
                Value = new Random().Next()
            };
            await _searchService.InsertSgv(toInsert);

            var response = await _client.GetAsync<Sgv>(toInsert.Id, i => i.Index(ElasticSearchService.EntryIndex));
            Assert.IsTrue(response.Found);
            Assert.AreEqual(toInsert.Value, response.Source.Value);
        }
    }
}
