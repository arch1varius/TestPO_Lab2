using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DummyTests
{
    public class StaticFilesTests : IClassFixture<WebApplicationFactory<AspnetCoreMvcFull.TestHostEntryPoint>>
    {
        private readonly WebApplicationFactory<AspnetCoreMvcFull.TestHostEntryPoint> _factory;

        public StaticFilesTests(WebApplicationFactory<AspnetCoreMvcFull.TestHostEntryPoint> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Get_MainJs_Returns_200_And_JavaScript_Content()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var res = await client.GetAsync("/js/main.js");
            var body = await res.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            Assert.Contains("javascript", res.Content.Headers.ContentType?.MediaType ?? string.Empty);
            Assert.False(string.IsNullOrWhiteSpace(body));
            Assert.Contains("Main", body);
        }

        [Fact]
        public async Task Get_Nonexistent_File_Returns_404()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var res = await client.GetAsync("/js/does-not-exist.js");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }

        [Fact]
        public async Task Get_Trophy_Image_Returns_Image_Content_And_Has_Length()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var res = await client.GetAsync("/img/illustrations/trophy.png");

            // Assert
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var mediaType = res.Content.Headers.ContentType?.MediaType ?? string.Empty;
            Assert.StartsWith("image/", mediaType);
            var bytes = await res.Content.ReadAsByteArrayAsync();
            Assert.True(bytes.Length > 0, "Expected image response to contain bytes");
        }
    }
}
