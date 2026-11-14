using KinderHub.Application.Interfaces;
using KinderHub.Web.Health;
using KinderHub.Web.Realtime;
using KinderHub.Web.Services;
using KinderHub.Web.State;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KinderHub.Web;

// Registers all services that belong to the Web (UI) layer.
public static class WebServices
{
    public static IServiceCollection AddKinderHubWeb(IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<AppState>();
        services.AddScoped<ToastService>();
        services.TryAddScoped<ProtectedLocalStorage>();

        // Real-time: SignalR + one shared notifier for the whole app
        services.AddSignalR();
        services.AddSingleton<IRealtimeNotifier, RealtimeNotifier>();

        // Health checks: /health tells if the app and the database are OK
        services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");

        // API documentation: OpenAPI description at /openapi/v1.json
        services.AddOpenApi();

        return services;
    }
}
