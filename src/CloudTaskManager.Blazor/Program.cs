using Blazored.LocalStorage;
using CloudTaskManager.Blazor;
using CloudTaskManager.Blazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API base URL (adjust for your backend localhost port)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5113/") });
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, FakeAuthStateProvider>();
builder.Services.AddScoped<NotificationHubClient>();

await builder.Build().RunAsync();