using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
namespace CloudTaskManager.Blazor.Services;


public class FakeAuthStateProvider(ILocalStorageService localStorage) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var username = await localStorage.GetItemAsync<string>("userId");
        if (string.IsNullOrEmpty(username))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Name, username)
        ], "FakeAuth");

        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }

    public void NotifyUserChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
