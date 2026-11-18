using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Infrastructure.Authentication;

public class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
{
    public (string AccessToken, DateTime ExpiresAt) GenerateToken(Guid userId, string fullName, bool isSuperAdmin, Guid? officeId)
    {
        var key = configuration["Jwt:Key"]!; // Program.cs's startup guard already proved this is set.
        var expiresAt = DateTime.UtcNow.AddHours(8);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, fullName),
            new("is_super_admin", isSuperAdmin ? "true" : "false"),
        };
        if (officeId is not null) claims.Add(new Claim("office_id", officeId.Value.ToString()));

        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
