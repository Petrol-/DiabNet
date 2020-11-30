using System;
using System.Net.Http;
using System.Threading.Tasks;
using DiabNet.Features.Nightscout;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace DiabNet.TestIT.Features.Nightscout
{
    public class NightscoutApiTest
    {
        
        private Mock<IHttpClientFactory> _factory;
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
            _factory = new Mock<IHttpClientFactory>();
            _factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient
            {
                BaseAddress = new Uri(_nightscoutUrl)
            });
            _api = new NightscoutApi(_factory.Object);
        }

        [Test]
        public async Task GetEntryShouldReturnEntries()
        {
            Assert.IsNotEmpty(await _api.GetEntries());
        }
    }
}
