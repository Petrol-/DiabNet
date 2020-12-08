using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace DiabNet.Nightscout.TestIT
{
    public class NightscoutApiTest
    {
        
        private NightscoutApi _api;
        private string _nightscoutUrl;
        [OneTimeSetUp]
        public void Init()
        {
            var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            _nightscoutUrl= configuration["NIGHTSCOUT_URL"];
        }

        [SetUp]
        public void Setup()
        {
            _api = new NightscoutApi(new HttpClient
            {
                BaseAddress = new Uri(_nightscoutUrl)
            });
        }

        [Test]
        public async Task GetEntryShouldReturnEntries()
        {
            Assert.IsNotEmpty(await _api.GetEntries());
        }
    }
}
