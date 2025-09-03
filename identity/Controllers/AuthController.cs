using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using identity.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace identity.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(UserManager<IdentityUser> userManager,
    IConfiguration config) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> CreateUser(RegisterRequest registerRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var user = new IdentityUser()
        {
            UserName = registerRequest.UserName,
            Email = registerRequest.Email
        };
        var result = await userManager.CreateAsync(user, registerRequest.Password);
        
        if (result.Succeeded)
            return Ok(new { user.Id, Message = "User registered successfully" });

        return BadRequest(result.Errors);
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var user = await userManager.FindByEmailAsync(loginRequest.Email);
        
        if (user == null || !await userManager.CheckPasswordAsync(user, loginRequest.Password))
            return Unauthorized(new { Message = "Invalid email or password" });
        
        var token = await GenerateJwtToken(user);
        return Ok(new { Token = token });
    }
    
    private async Task<string> GenerateJwtToken(IdentityUser user)
    {
        var jwtSettings = config.GetSection("Jwt");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var roles = await userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roleClaims);


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