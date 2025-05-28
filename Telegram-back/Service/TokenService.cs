using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Telegram_back.Service
{
    public interface ITokenService
    {
        string GenerateJwtToken(string email, string name);
    }

    public class TokenService : ITokenService
    {
        private readonly string _secretKey;
        private readonly string _issuer;

        public TokenService(IConfiguration configuration)
        {
            _secretKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Secret Key is not configured.");
            _issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured.");
        }

        public string GenerateJwtToken(string email, string name)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
                throw new ArgumentNullException("Email and name must be provided.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, name),
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _issuer,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
