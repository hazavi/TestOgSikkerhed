using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using Xunit;

namespace TestOgSikkerhed.Tests
{
    public class HomeControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        // Constructor remains unchanged, but the accessibility issue is resolved by making CustomWebApplicationFactory public
        public HomeControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task Privacy_AsAdmin_ReturnsSuccess()
        {
            _client.DefaultRequestHeaders.Authorization =
                new
                System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.TestScheme);
            var response = await _client.GetAsync("/Home/Privacy");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Privacy_AsAnonymous_ReturnsRedirectToLogin()
        {
            var anonymousClient = new WebApplicationFactory<Program>()
                .CreateClient(new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });
            var response = await anonymousClient.GetAsync("/Home/Privacy");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", response.Headers.Location.ToString());
        }
    }
}
