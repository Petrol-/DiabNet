using System.Threading.Tasks;
using NUnit.Framework;

namespace DiabNet.ElasticSearch.TestIT
{
    [SetUpFixture]
    public class ExternalDependenciesInitializer
    {
        private readonly ElasticSearchTestContainer _elasticSearch = new();

        [OneTimeSetUp]
        public async Task Setup()
        {
            await _elasticSearch.Start();
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            await _elasticSearch.Stop();
        }
    }
}
