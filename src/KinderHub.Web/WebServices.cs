using KinderHub.Application.Interfaces;
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
        return services;
    }
}
