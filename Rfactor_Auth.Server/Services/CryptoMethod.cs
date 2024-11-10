using Konscious.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Rfactor_Auth.Server.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Rfactor_Auth.Server.Services
{
    public class CryptoMethod : ICrypto
    {
        private const int Iterations = 4;
        private const int MemorySize = 65536;
        private const int DegreeOfParallelism = 2;
        private readonly IConfiguration _configuration;

        public CryptoMethod(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string HashPassword(string password, byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                Iterations = Iterations,
                MemorySize = MemorySize,
                DegreeOfParallelism = DegreeOfParallelism
            };

            byte[] hash = argon2.GetBytes(32);
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        public string GenerateAccessToken(string username) => GenAT(username);
        private string GenAT(string username)
        {
            var claims = new[]
               {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, _configuration["JwtSettings:Algorithm"]);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(int.Parse(_configuration["JwtSettings:ExpiryMinutes"])),
                signingCredentials: creds);


            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken() => GenRT();
        private string GenRT()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
