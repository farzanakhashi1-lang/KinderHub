using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace KinderHub.Tests.Integration;

// Starts the REAL web app in memory, with its own fresh databases in a temp folder,
// so tests never touch your development data.
public class KinderHubAppFactory : WebApplicationFactory<Program>
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "kinderhub-tests-" + Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Directory.CreateDirectory(_folder);
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"DataSource={Path.Combine(_folder, "app.db")}");
        builder.UseSetting("ConnectionStrings:KinderHubConnection", $"DataSource={Path.Combine(_folder, "kinderhub.db")}");
    }
}

public class EndpointTests(KinderHubAppFactory factory) : IClassFixture<KinderHubAppFactory>
{
    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"status\": \"Healthy\"", body);
        Assert.Contains("\"children\": 12", body); // demo data was created at startup
    }

    [Fact]
    public async Task Home_page_loads_for_visitors()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("KinderHub", html);
        Assert.Contains("Demo accounts", html);
    }

    [Fact]
    public async Task Csv_export_requires_login()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/export/attendance.csv");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Admin_page_is_not_shown_to_visitors()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/admin/status");
        var html = await response.Content.ReadAsStringAsync();

        // Not logged in: the admin content must never be in the response (redirect to login instead)
        Assert.DoesNotContain("Overall health", html);
    }
}
