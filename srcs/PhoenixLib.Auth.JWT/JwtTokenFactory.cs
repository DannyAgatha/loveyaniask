using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PhoenixLib.Auth.JWT
{
    public class JwtTokenFactory : IJwtTokenFactory
    {
        private const string SigningKeyAlgorithm = SecurityAlgorithms.HmacSha256Signature;
        private const string SigningKeyAlgorithmShortName = "HS256"; //Yeah, Idk how to obtain this, but it basically represents the algorithm we use.
        private readonly string _audience;
        private readonly string _issuer;
        private readonly string _jwtPrivateKey;
        private readonly TimeSpan _tokenLifeTime;

        public JwtTokenFactory(string jwtPrivateKey, string issuer = null, string audience = null, TimeSpan? tokenLifeTime = null)
        {
            _jwtPrivateKey = jwtPrivateKey;
            _issuer = issuer ?? "https://noswings.com";
            _audience = audience ?? "https://noswings.com";
            _tokenLifeTime = tokenLifeTime ?? TimeSpan.FromHours(1);
        }

        private SecurityKey GetSigningKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtPrivateKey.ToSha512()));

        public string CreateJwtToken() => CreateJwtToken(Array.Empty<Claim>());

        public string CreateJwtToken(params Claim[] claims)
        {
            SecurityKey signinKey = GetSigningKey;
            var handler = new JwtSecurityTokenHandler();

            var claimsIdentity = new ClaimsIdentity(claims);
            SecurityToken securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Issuer = _issuer,
                Audience = _audience,
                Expires = DateTime.UtcNow + _tokenLifeTime,
                SigningCredentials = new SigningCredentials(signinKey, SigningKeyAlgorithm)
            });
            return handler.WriteToken(securityToken);
        }

        public IEnumerable<Claim> ObtainClaimsFromJwtToken(string jwtToken, TokenValidationParameters validationParameters)
        {
            validationParameters.ValidIssuer = _issuer;
            validationParameters.ValidAudience = _audience;
            validationParameters.ValidAlgorithms = new[]
            {
                SigningKeyAlgorithmShortName
            };
            validationParameters.IssuerSigningKey = GetSigningKey;
            if (validationParameters.ValidateLifetime)
            {
                validationParameters.LifetimeValidator = LifetimeValidator;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(jwtToken, validationParameters, out _);

            return claimsPrincipal.Claims;
        }

        private static bool LifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if (expires == null)
            {
                return false;
            }

            return DateTime.UtcNow < expires;
        }
    }
}