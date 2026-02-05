using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace PhoenixLib.Auth.JWT
{
    public interface IJwtTokenFactory
    {
        string CreateJwtToken();
        string CreateJwtToken(params Claim[] claims);
        IEnumerable<Claim> ObtainClaimsFromJwtToken(string jwtToken, TokenValidationParameters validationParameters);
    }
}