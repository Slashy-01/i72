using I72_Backend.Interfaces;
using I72_Backend.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace I72_Backend.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Generate a refresh token with cryptographically secure random bytes
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        // Get claims principal from an expired token
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, // We do not validate the audience for expired tokens
                ValidateIssuer = false,   // Issuer validation is disabled here as well
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateLifetime = false  // Lifetime validation is disabled for expired tokens
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal; // Return the claims principal from the expired token
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Error validating token: " + ex.Message);
            }
        }

        // Generate a new JWT token for a user
        public string GenerateNewToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiresInMinutes"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token); // Return the JWT token as a string
        }

        // Encode a refresh token using BCrypt
        public string EncodeRefreshToken(string refreshToken)
        {
            // Hash the refresh token securely before storing it
            return BCrypt.Net.BCrypt.HashPassword(refreshToken);
        }

        // Validate a provided refresh token against the stored hash
        public bool ValidateRefreshToken(string storedHash, string providedToken)
        {
            // Compare the hashed version of the provided token with the stored hash
            return BCrypt.Net.BCrypt.Verify(providedToken, storedHash);
        }
    }
}
