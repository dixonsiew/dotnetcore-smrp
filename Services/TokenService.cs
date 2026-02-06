using Microsoft.IdentityModel.Tokens;
using smrp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace smrp.Services
{
    // https://medium.com/@MatinGhanbari/building-a-secure-api-with-asp-net-core-jwt-and-refresh-tokens-03dac37b4055
    // https://medium.com/@emreemenekse/a-comprehensive-guide-to-jwt-authentication-in-net-core-8e2d8859b1be#id_token=eyJhbGciOiJSUzI1NiIsImtpZCI6ImMyN2JhNDBiMDk1MjlhZDRmMTY4MjJjZTgzMTY3YzFiYzM5MTAxMjIiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLCJhenAiOiIyMTYyOTYwMzU4MzQtazFrNnFlMDYwczJ0cDJhMmphbTRsamRjbXMwMHN0dGcuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJhdWQiOiIyMTYyOTYwMzU4MzQtazFrNnFlMDYwczJ0cDJhMmphbTRsamRjbXMwMHN0dGcuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJzdWIiOiIxMDgxMDY5NTMxNjU4ODA5NjI1NDEiLCJlbWFpbCI6ImRpeG9uc2lld0BnbWFpbC5jb20iLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwibm9uY2UiOiJub3RfcHJvdmlkZWQiLCJuYmYiOjE3NzAzMDUwMzUsIm5hbWUiOiJEaXhvbiBTaWV3IiwicGljdHVyZSI6Imh0dHBzOi8vbGgzLmdvb2dsZXVzZXJjb250ZW50LmNvbS9hL0FDZzhvY0xyQ2RJM2h4WENJOTgwUU95dkdkcTVuRWtKTGdtMjZ4aUk4NUhWdVpZN3htWm9iWTlaPXM5Ni1jIiwiZ2l2ZW5fbmFtZSI6IkRpeG9uIiwiZmFtaWx5X25hbWUiOiJTaWV3IiwiaWF0IjoxNzcwMzA1MzM1LCJleHAiOjE3NzAzMDg5MzUsImp0aSI6IjYzN2NmZGVkZjg1ZDI0ZjNkZWIyMDhhNTYwMjFmMzNhOGZmMTVmOTUifQ.uB-JuFpbJzXQr2Ss02oaGufOvaFnhlE7nYdt5FcDK_bh3MYo7yPzEAcUTB1pF4NyEoLRi6u2py3071iVvGnLMOYU05TTlf3Iczgxqdm4RL56_obNHB5-GkraW4rj9Dw46RKeg-5j13rhXIicSZqDI2n97RCT6SX6hwQWqycYJOkSRMDoSQ3Y9LgwpjG7cb1tg27mb_dvmfZ9qVpWHrElYdRbi83w6LNVCD2eHyBzEn2ht-qG_eLMVRgZQdfTd7dWH8_21MGeOCzopPP9tJSM5lkYtd8OMSacSZ0l720GNq88ZSgZ0xgkf78EnMfL5ZMubMQa4iX45mOX8QlSIb2jDg
    public class TokenService
    {
        private readonly IConfiguration config;

        public TokenService(IConfiguration cfg)
        {
            config = cfg;
        }

        public string GenerateAccessToken(User user)
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? ""));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            Claim[] claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Username),
            };
            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(720),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken(User user)
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? ""));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            Claim[] claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Username),
            };
            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(87600),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? "")),
                ValidateLifetime = false // We want to get claims from expired token
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
