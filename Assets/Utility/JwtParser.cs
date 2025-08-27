//using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
//using System.IdentityModel.Tokens.Jwt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace iBMSApp.Utility
{
    public class JwtParser
    {
        public static IEnumerable<Claim> ParseClaimsFromJwt(string authToken)
        {
            /*
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(authToken);

            var _claims = jwt.Claims.ToList();

            Dictionary<object, object> result = new Dictionary<object, object>();
            foreach (var claim in _claims)
            {
                result.Add(claim.ToString().Trim().Split(":")[0], claim.ToString().Trim().Split(":")[1]);
            }

            var claims = new List<Claim>();
            return claims;
            */
            
            List<Claim> claims = new List<Claim>();
            var payload = authToken.Split('.')[1];

            var jsonBytes = ParseBase64WithoutPadding(payload);

            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs != null)
            {
#nullable enable
                keyValuePairs.TryGetValue(ClaimTypes.Role, out object? roles);
#nullable disable

                if (roles != null)
                {
                    if ((roles.ToString() ?? "").Trim().StartsWith("["))//處理多角色問題
                    {
                        var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString() ?? "");
                        if (parsedRoles != null)
                        {
                            foreach (var parsedRole in parsedRoles)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                            }
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, roles.ToString() ?? ""));
                    }

                    keyValuePairs.Remove(ClaimTypes.Role);
                }
                claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? "")));
            }

            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            base64 = base64.Replace('-', '+');//The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.
            base64 = base64.Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
