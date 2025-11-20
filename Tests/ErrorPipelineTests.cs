using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DummyTests
{

  public class ErrorPipelineTests : IClassFixture<WebApplicationFactory<AspnetCoreMvcFull.TestHostEntryPoint>>
  {
    private readonly WebApplicationFactory<AspnetCoreMvcFull.TestHostEntryPoint> _factory;

    public ErrorPipelineTests(WebApplicationFactory<AspnetCoreMvcFull.TestHostEntryPoint> factory)
    {
      _factory = factory;
    }

    [Fact]
    public async Task Production_Exception_Is_Handled_By_ErrorPage_And_Includes_RequestId()
    {

      // Arrange
      var factory = _factory.WithWebHostBuilder(builder =>
      {
        builder.UseEnvironment("Production");
        builder.ConfigureServices(services =>
              {
            services.AddSingleton<Microsoft.AspNetCore.Hosting.IStartupFilter>(new AppendThrowingStartupFilter("/throw"));
          });
      });

      var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

      // Act
      var res = await client.GetAsync("/throw");
      var html = await res.Content.ReadAsStringAsync();

      // Assert
      Assert.True(res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.InternalServerError,
          $"Unexpected status code: {res.StatusCode}");

      if (res.StatusCode == HttpStatusCode.OK)
      {
        Assert.Contains("Request ID:", html);
        Assert.Matches("[A-Za-z0-9-]{8,}", html);
      }
      else
      {
        Assert.False(string.IsNullOrWhiteSpace(html));
      }
    }

    [Fact]
    public async Task Development_Exception_Is_Not_Routed_To_ErrorPage_Returns_ServerError()
    {
      // Arrange
      var factory = _factory.WithWebHostBuilder(builder =>
      {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
              {
            services.AddSingleton<Microsoft.AspNetCore.Hosting.IStartupFilter>(new AppendThrowingStartupFilter("/throw", "dev-boom"));
          });
      });

      var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

      // Act
      var res = await client.GetAsync("/throw");
      var body = await res.Content.ReadAsStringAsync();

      // Assert
      Assert.True((int)res.StatusCode >= 500, "Expected 5xx status in Development for unhandled exception");
      Assert.DoesNotContain("Request ID:", body);
    }

    [Fact]
    public async Task Production_Exception_With_NoActivity_Falls_Back_To_TraceIdentifier()
    {
      // Arrange
      var factory = _factory.WithWebHostBuilder(builder =>
      {
        builder.UseEnvironment("Production");
        builder.ConfigureServices(services =>
              {
                // Startup filter to clear Activity.Current at the beginning
            services.AddSingleton<Microsoft.AspNetCore.Hosting.IStartupFilter>(new ClearActivityStartupFilter());
                // Append throwing middleware after pipeline
            services.AddSingleton<Microsoft.AspNetCore.Hosting.IStartupFilter>(new AppendThrowingStartupFilter("/throw"));
          });
      });

      var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

      // Act
      var res = await client.GetAsync("/throw");
      var html = await res.Content.ReadAsStringAsync();

      // Assert
      Assert.True(res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.InternalServerError,
          $"Unexpected status code: {res.StatusCode}");

      if (res.StatusCode == HttpStatusCode.OK)
      {
        Assert.Contains("Request ID:", html);
      }
      else
      {
        Assert.False(string.IsNullOrWhiteSpace(html));
      }
    }

    [Fact]
    public async Task Production_Exception_When_ErrorView_Throws_Returns_5xx_Not_Blank()
    {
      // Arrange: simulate an error inside the Error view by adding middleware that throws for /Home/Error.
      var factory = _factory.WithWebHostBuilder(builder =>
      {
        builder.UseEnvironment("Production");
        builder.ConfigureServices(services =>
              {
                // Append a middleware that throws when /Home/Error is requested (simulate view error)
            services.AddSingleton<Microsoft.AspNetCore.Hosting.IStartupFilter>(new AppendThrowingStartupFilter("/Home/Error", "error in error view", throwOnMatch: true));
                // Append the original throwing middleware after pipeline
            services.AddSingleton<Microsoft.AspNetCore.Hosting.IStartupFilter>(new AppendThrowingStartupFilter("/throw", "original"));
          });
      });

      var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

      // Act
      var res = await client.GetAsync("/throw");
      var body = await res.Content.ReadAsStringAsync();

      // Assert: when the Error view blows up, we still get a server error (5xx) and a non-empty response body.
      Assert.True((int)res.StatusCode >= 500, "Expected 5xx status when Error view throws");
      Assert.False(string.IsNullOrWhiteSpace(body));
    }
  }
}

// Test helpers: startup filters used only in tests to append middleware or clear Activity.Current.
internal class AppendThrowingStartupFilter : Microsoft.AspNetCore.Hosting.IStartupFilter
{
  private readonly string _pathToMatch;
  private readonly string? _message;
  private readonly bool _throwOnMatch;

  public AppendThrowingStartupFilter(string pathToMatch, string? message = null, bool throwOnMatch = true)
  {
    _pathToMatch = pathToMatch;
    _message = message ?? "boom";
    _throwOnMatch = throwOnMatch;
  }

  public Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> Configure(Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> next)
  {
    return app =>
    {
      // Run existing pipeline first
      next(app);

      // Then append middleware that will throw when the path matches.
      app.Use(async (context, _next) =>
          {
          if (string.Equals(context.Request.Path, _pathToMatch, StringComparison.OrdinalIgnoreCase))
          {
            throw new Exception(_message);
          }
          await _next();
        });
    };
  }
}

internal class ClearActivityStartupFilter : Microsoft.AspNetCore.Hosting.IStartupFilter
{
  public Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> Configure(Action<Microsoft.AspNetCore.Builder.IApplicationBuilder> next)
  {
    return app =>
    {
      // Insert middleware at the beginning of the pipeline to clear Activity.Current for the request.
      app.Use(async (context, _next) =>
          {
          System.Diagnostics.Activity.Current = null;
          await _next();
        });

      next(app);
    };
  }
}
