using Microsoft.IdentityModel.Tokens;
using Project1.ResourceModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Project1.Services
{
    public class JwtService
    {
        private const int EXPIRATION_MINUTES = 21600; // 15 days

        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AuthenticationResponse CreateToken(IdentityUser user)
        {
            var expiration = DateTime.UtcNow.AddMinutes(EXPIRATION_MINUTES);

            var token = CreateJwtToken(
                CreateClaims(user),
                CreateSigningCredentials(),
                expiration
            );

            var tokenHandler = new JwtSecurityTokenHandler();

            return new AuthenticationResponse
            {
                Token = tokenHandler.WriteToken(token),
                Expiration = expiration
            };
        }

        private JwtSecurityToken CreateJwtToken(Claim[] claims, SigningCredentials credentials, DateTime expiration) =>
            new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expiration,
                signingCredentials: credentials
            );

        private Claim[] CreateClaims(IdentityUser user) =>
            new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("DateOfJoining", DateTime.UtcNow.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber)
            };

        private SigningCredentials CreateSigningCredentials() =>
            new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
                ),
                SecurityAlgorithms.HmacSha256
            );
    }
}
