using System.Security.Claims;
using KinderHub.Application.Common;
using KinderHub.Application.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace KinderHub.Web.Services;

// Reads the logged-in user from Blazor's authentication state
public class CurrentUserService(AuthenticationStateProvider authStateProvider) : ICurrentUserService
{
    public async Task<CurrentUserInfo> GetCurrentUserAsync()
    {
        var state = await authStateProvider.GetAuthenticationStateAsync();
        var user = state.User;

        if (user.Identity?.IsAuthenticated != true)
            return CurrentUserInfo.Anonymous;

        var email = user.FindFirstValue(ClaimTypes.Email) ?? user.Identity.Name ?? string.Empty;
        var name = user.FindFirstValue(KinderHubClaims.FullName) ?? email;
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return new CurrentUserInfo(true, email, name, roles);
    }
}
