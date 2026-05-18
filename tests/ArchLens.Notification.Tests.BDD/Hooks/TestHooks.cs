using System.Security.Claims;
using System.Text.Encodings.Web;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Reqnroll;
using StackExchange.Redis;

namespace ArchLens.Notification.Tests.BDD.Hooks;

public sealed class BddWebApplicationFactory : WebApplicationFactory<ArchLens.Notification.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "SuperSecretTestKeyThatIsLongEnoughForHmacSha256!!",
                ["Jwt:Issuer"] = "archlens-auth",
                ["Jwt:Audience"] = "archlens-services",
                ["Redis:ConnectionString"] = "localhost:6379",
                ["RabbitMQ:Host"] = "localhost",
                ["RabbitMQ:Username"] = "guest",
                ["RabbitMQ:Password"] = "guest",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove all Redis-related services (SignalR backplane, health checks)
            var redisDescriptors = services
                .Where(d =>
                    (d.ServiceType.FullName?.Contains("Redis") == true) ||
                    (d.ImplementationType?.FullName?.Contains("Redis") == true) ||
                    (d.ServiceType.FullName?.Contains("StackExchange") == true) ||
                    (d.ImplementationType?.FullName?.Contains("StackExchange") == true) ||
                    (d.ServiceType == typeof(IConnectionMultiplexer)))
                .ToList();
            foreach (var descriptor in redisDescriptors)
            {
                services.Remove(descriptor);
            }

            // Remove all health check registrations and re-add without Redis
            var healthDescriptors = services
                .Where(d =>
                    d.ServiceType == typeof(HealthCheckService) ||
                    d.ServiceType == typeof(IHealthCheck) ||
                    d.ServiceType.FullName?.Contains("HealthCheck") == true)
                .ToList();
            foreach (var descriptor in healthDescriptors)
            {
                services.Remove(descriptor);
            }

            // Clear existing health check registrations
            services.Configure<HealthCheckServiceOptions>(options =>
            {
                options.Registrations.Clear();
            });

            // Remove all hosted services (MassTransit bus, Redis connection, etc.)
            var hostedServices = services
                .Where(d => d.ServiceType == typeof(IHostedService))
                .ToList();
            foreach (var descriptor in hostedServices)
            {
                services.Remove(descriptor);
            }

            // Remove all MassTransit services and replace with test harness
            var massTransitDescriptors = services
                .Where(d =>
                    (d.ServiceType.FullName?.Contains("MassTransit") == true) ||
                    (d.ImplementationType?.FullName?.Contains("MassTransit") == true) ||
                    (d.ServiceType == typeof(IBus)) ||
                    (d.ServiceType == typeof(IBusControl)) ||
                    (d.ServiceType == typeof(IPublishEndpoint)) ||
                    (d.ServiceType == typeof(ISendEndpointProvider)))
                .ToList();
            foreach (var descriptor in massTransitDescriptors)
            {
                services.Remove(descriptor);
            }
            services.AddMassTransitTestHarness();

            // Remove SignalR backplane services and re-add SignalR without Redis
            var signalrBackplaneDescriptors = services
                .Where(d =>
                    d.ServiceType.FullName?.Contains("SignalR") == true &&
                    (d.ImplementationType?.FullName?.Contains("Redis") == true ||
                     d.ImplementationType?.FullName?.Contains("StackExchange") == true))
                .ToList();
            foreach (var descriptor in signalrBackplaneDescriptors)
            {
                services.Remove(descriptor);
            }

            // Remove existing authentication and replace with test scheme
            services.RemoveAll<IConfigureOptions<AuthenticationOptions>>();
            services.RemoveAll<IPostConfigureOptions<JwtBearerOptions>>();
            services.RemoveAll<IConfigureOptions<JwtBearerOptions>>();
            services.RemoveAll<IPostConfigureOptions<AuthenticationOptions>>();

            var authDescriptors = services
                .Where(d =>
                    d.ServiceType.FullName?.Contains("JwtBearer") == true ||
                    (d.ImplementationType?.FullName?.Contains("JwtBearer") == true))
                .ToList();
            foreach (var descriptor in authDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
                options.DefaultScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, BddTestAuthHandler>("TestScheme", _ => { });
        });
    }
}

public sealed class BddTestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public static bool IsAuthenticated { get; set; } = true;
    public static string UserId { get; set; } = "test-user-id";
    public static string UserEmail { get; set; } = "test@archlens.com";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!IsAuthenticated)
            return Task.FromResult(AuthenticateResult.Fail("Not authenticated"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, UserId),
            new Claim(ClaimTypes.Email, UserEmail),
            new Claim(ClaimTypes.Name, "Test User"),
        };

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

[Binding]
public sealed class TestHooks
{
    private static BddWebApplicationFactory _factory = null!;
    private static HttpClient _client = null!;

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        _factory = new BddWebApplicationFactory();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [BeforeScenario]
    public void BeforeScenario(ScenarioContext scenarioContext)
    {
        BddTestAuthHandler.IsAuthenticated = true;
        BddTestAuthHandler.UserId = "test-user-id";
        BddTestAuthHandler.UserEmail = "test@archlens.com";

        scenarioContext["HttpClient"] = _client;
    }
}
