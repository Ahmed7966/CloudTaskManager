using System.Text;
using CloudTaskManager.Gateway.Middleware.Extensions;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

builder.Services.AddAuthorization();
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins!) 
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(transformContext =>
        {
            if (!transformContext.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
                return ValueTask.CompletedTask;

            transformContext.ProxyRequest.Headers.Remove("Authorization");
            transformContext.ProxyRequest.Headers.Add("Authorization", authHeader.ToString());
            return ValueTask.CompletedTask;
        });
    });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("BlazorPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseCorrelationId();
app.MapReverseProxy();
app.MapGet("/", () => "Hello World!");

app.Run();