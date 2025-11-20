using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace DummyTests
{
  // Integration tests using WebApplicationFactory<Program>.
  // Logic:
  // - Start the real app pipeline in-memory and send HTTP requests against it.
  // - For PathBase test we inject a small middleware to set Request.PathBase so the layout
  //   (which reads IHttpContextAccessor.HttpContext.Request.PathBase) receives a non-empty value.
  // - For static file test we request a known asset under wwwroot and assert 200 + content-type.
  // - For routing test we request the root and assert the Dashboard view content is returned.

  public class IntegrationTests : IClassFixture<WebApplicationFactory<AspnetCoreMvcFull.TestHostEntryPoint>>
  {
    private readonly WebApplicationFactory<AspnetCoreMvcFull.TestHostEntryPoint> _factory;

    public IntegrationTests(WebApplicationFactory<AspnetCoreMvcFull.TestHostEntryPoint> factory)
    {
      _factory = factory;
    }

    [Fact]
    public async Task Layout_Includes_PathBase_When_Set()
    {
      // Arrange: create a factory that sets PathBase via middleware before routing.
      var factory = _factory.WithWebHostBuilder(builder =>
      {
        // Register an IStartupFilter so the PathBase-setting middleware runs at the
        // start of the pipeline (before routing / HTTPS redirection etc.).
        builder.ConfigureServices(services =>
        {
          services.AddSingleton<Microsoft.AspNetCore.Hosting.IStartupFilter>(new PathBaseStartupFilter(new PathString("/app")));
        });
      });

      var client = factory.CreateClient();

      // Act
      var res = await client.GetAsync("/");
      var html = await res.Content.ReadAsStringAsync();

      // Assert
      Assert.Equal(HttpStatusCode.OK, res.StatusCode);
      // PathBase should be applied to asset URLs in the rendered HTML; verify a known asset path includes the prefix.
      Assert.Contains("/app/js/dashboards-analytics.js", html);
    }

    [Fact]
    public async Task StaticFile_MainJs_IsServed()
    {
      var client = _factory.CreateClient();

      var res = await client.GetAsync("/js/main.js");

      Assert.Equal(HttpStatusCode.OK, res.StatusCode);
      Assert.Contains("javascript", res.Content.Headers.ContentType?.MediaType ?? string.Empty);
      var text = await res.Content.ReadAsStringAsync();
      Assert.Contains("Main", text);
    }

    [Fact]
    public async Task DefaultRoute_Returns_DashboardsIndex_View()
    {
      var client = _factory.CreateClient();

      var res = await client.GetAsync("/");

      Assert.Equal(HttpStatusCode.OK, res.StatusCode);
      var html = await res.Content.ReadAsStringAsync();
      // Known text fragment from Views/Dashboards/Index.cshtml
      Assert.Contains("Congratulations Norris", html);
    }
  }
}

// A simple IStartupFilter that inserts middleware at the beginning of the pipeline
// to set Request.PathBase for incoming requests. Used only in tests via ConfigureTestServices.
internal class PathBaseStartupFilter : Microsoft.AspNetCore.Hosting.IStartupFilter
{
  private readonly PathString _pathBase;
  public PathBaseStartupFilter(PathString pathBase) => _pathBase = pathBase;

  public System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> Configure(System.Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> next)
  {
    return app =>
    {
      app.Use(async (context, _next) =>
      {
        context.Request.PathBase = _pathBase;
        await _next();
      });

      next(app);
    };
  }
}
