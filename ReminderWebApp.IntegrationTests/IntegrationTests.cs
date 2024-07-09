using Microsoft.AspNetCore.Mvc.Testing;

namespace ReminderWebApp.IntegrationTests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _applicationFactory;
        public IntegrationTests(WebApplicationFactory<Program> applicationFactory)
        {
            _applicationFactory = applicationFactory;
        }

        [Fact]
        public async Task HealthzRequest_ReturnHealthy()
        {
            HttpClient client = _applicationFactory.CreateClient();
            var response = await client.GetAsync("/healthz");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Healthy", content);
        }
    }
}
