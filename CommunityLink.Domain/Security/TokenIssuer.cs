using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CommunityLink.Shared;
using Microsoft.IdentityModel.Tokens;

namespace CommunityLink.Domain.Security;

public interface ITokenIssuer
{
    string GenerateToken(int userId, string email, string fullName, string roleCode, int roleId, string sessionId);
}

public sealed class JwtTokenIssuer(CustomSettingModel settings) : ITokenIssuer
{
    public string GenerateToken(int userId, string email, string fullName, string roleCode, int roleId, string sessionId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, fullName),
            new("name", fullName),
            new(ClaimTypes.Role, roleCode),
            new("role", roleCode),
            new("role_id", roleId.ToString()),
            new("session_id", sessionId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: settings.Jwt.Issuer,
            audience: settings.Jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(settings.Jwt.ExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}