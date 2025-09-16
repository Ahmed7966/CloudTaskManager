using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CloudTaskManager.Identity.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CloudTaskManager.Identity.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<IdentityUser> userManager,
    IConfiguration config,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> CreateUser(RegisterRequest registerRequest)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid registration request [CorrelationId: {CorrelationId}]",
                HttpContext.Items["X-Correlation-Id"]);
            return BadRequest(ModelState);
        }

        var user = new IdentityUser()
        {
            UserName = registerRequest.UserName,
            Email = registerRequest.Email
        };
        var result = await userManager.CreateAsync(user, registerRequest.Password);

        if (result.Succeeded)
        {
            logger.LogInformation("User {UserId} registered successfully [CorrelationId: {CorrelationId}]",
                user.Id,
                HttpContext.Items["X-Correlation-Id"]);
            await userManager.AddToRoleAsync(user, "User");
            return Ok(new { user.Id, Message = "User registered successfully" });
        }

        logger.LogError("Failed to register user {UserName}. Errors: {Errors} [CorrelationId: {CorrelationId}]",
            user.UserName,
            string.Join(",", result.Errors.Select(e => e.Description)),
            HttpContext.Items["X-Correlation-Id"]);
        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid login request [CorrelationId: {CorrelationId}]",
                HttpContext.Items["X-Correlation-Id"]);
            return BadRequest(ModelState);
        }

        var user = await userManager.FindByEmailAsync(loginRequest.Email);

        if (user == null || !await userManager.CheckPasswordAsync(user, loginRequest.Password))
        {
            logger.LogWarning("Failed login attempt for email {Email} [CorrelationId: {CorrelationId}]",
                loginRequest.Email,
                HttpContext.Items["X-Correlation-Id"]);

            return Unauthorized(new { Message = "Invalid email or password" });
        }

        var token = await GenerateJwtToken(user);

        logger.LogInformation("User {UserId} logged in successfully [CorrelationId: {CorrelationId}]",
            user.Id,
            HttpContext.Items["X-Correlation-Id"]);

        return Ok(new { Token = token });
    }


    private async Task<string> GenerateJwtToken(IdentityUser user)
    {
        var jwtSettings = config.GetSection("Jwt");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName!)
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}