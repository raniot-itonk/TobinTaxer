using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace TobinTaxer.IntegrationTests
{
    public class ControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public ControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task TestTaxRate()
        {
            var response = await _client.GetAsync("/api/TaxRate");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal(0.01, double.Parse(result));
        }
    }
}
