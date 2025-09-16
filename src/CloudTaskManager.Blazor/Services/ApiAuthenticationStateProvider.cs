using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
namespace CloudTaskManager.Blazor.Services;

public class ApiAuthenticationStateProvider(HttpClient http) : AuthenticationStateProvider
{
    private string? _token;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = string.IsNullOrEmpty(_token)
            ? new ClaimsIdentity()
            : new ClaimsIdentity(ParseClaimsFromJwt(_token), "jwt");

        var user = new ClaimsPrincipal(identity);
        return await Task.FromResult(new AuthenticationState(user));
    }

    public void SetToken(string? token)
    {
        _token = token;

        http.DefaultRequestHeaders.Authorization =
            !string.IsNullOrEmpty(token) ? 
                new AuthenticationHeaderValue("Bearer", token) : null;

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = Convert.FromBase64String(PadBase64(payload));
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        return keyValuePairs!.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? ""));
    }

    private static string PadBase64(string base64)
    {
        if (string.IsNullOrEmpty(base64))
            return base64;

        return (base64.Length % 4) switch
        {
            0 => base64,
            2 => base64 + "==",
            3 => base64 + "=",
            1 => throw new FormatException("Invalid Base64 string length."),
            _ => base64 
        };
    }
}
