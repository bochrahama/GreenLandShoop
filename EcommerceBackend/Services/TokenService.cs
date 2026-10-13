using EcommerceBackend.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EcommerceBackend.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<AppUser> _userManager;
        public TokenService(IConfiguration config, UserManager<AppUser> userManager)
        {
            _config = config;
            _userManager = userManager;
        }
        public async Task<String> CreateTokenAsync(AppUser User)
        {
            var roles = await _userManager.GetRolesAsync(User);
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier , User.Id),
                new(ClaimTypes.Email , User.Email ??String.Empty),
                new(ClaimTypes.Name , User.FullName)
            };
            // Add roles to claims to include them in the JWT token
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            //HmacSha512Signature is a hashing algorithm used for signing the JWT token to ensure its integrity and authenticity.
            var creds = new SigningCredentials(Key, SecurityAlgorithms.HmacSha512Signature);
            // the porpes of the JwtSecurityToken constructor is to create a new JWT token with the specified issuer, audience, claims, expiration time, and signing credentials.
            // The resulting token can then be serialized and sent to the client for authentication and authorization purposes.
            var Token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpirationInMinutes"]!)),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(Token);
        }
    }
}