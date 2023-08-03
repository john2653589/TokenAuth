using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Rugal.TokenAuth.Common.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Rugal.TokenAuth.Common.Service
{
    public partial class TokenService
    {
        private readonly JwtParamModel JwtParam;
        private readonly TokenAuthSetting Setting;
        private readonly IServiceProvider ServiceProvider;
        public TokenService(JwtParamModel _JwtParam, TokenAuthSetting _Setting, IServiceProvider _ServiceProvider)
        {
            JwtParam = _JwtParam;
            Setting = _Setting;
            ServiceProvider = _ServiceProvider;
        }
        public string CreateToken(IdentityUser User, IList<string> Roles, Dictionary<string, object> ClaimsDic)
        {
            var NowTime = DateTime.Now;

            var SigningKeyValue = JwtParam.IssuerSigningKey;
            var SigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKeyValue));

            var RolesString = string.Join(",", Roles);
            var Claims = new Dictionary<string, object>
            {
                { JwtRegisteredClaimNames.Sub, User.Id },
                { "UserId", User.Id },
                { "Roles", RolesString },
            };

            foreach (var Item in ClaimsDic)
                Claims.Add(Item.Key, Item.Value);

            var SignCred = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);
            var JwtDescrip = new SecurityTokenDescriptor()
            {
                Issuer = JwtParam.Issuer,
                Audience = JwtParam.Audience,
                Claims = Claims,
                NotBefore = NowTime,
                Expires = NowTime.AddHours(JwtParam.TokenExpiresHours),
                SigningCredentials = SignCred,
            };

            var TokenCreater = new JwtSecurityTokenHandler();
            var SecurityToken = TokenCreater.CreateToken(JwtDescrip);
            var Token = TokenCreater.WriteToken(SecurityToken);

            return Token;
        }
        public bool ValidateToken(string Token, out ClaimsPrincipal User)
        {
            User = null;
            if (Token == null)
                return false;

            if (Token.Contains(' '))
                Token = Token.Split(' ')[1];

            var SigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtParam.IssuerSigningKey));
            try
            {
                var TokenParam = new TokenValidationParameters
                {
                    IssuerSigningKey = SigningKey,
                    ValidIssuer = JwtParam.Issuer,
                    ValidAudience = JwtParam.Audience,

                    ValidateIssuerSigningKey = JwtParam.ValidateIssuerSigningKey,
                    ValidateIssuer = JwtParam.ValidateIssuer,
                    ValidateAudience = JwtParam.ValidateAudience,
                    ValidateLifetime = JwtParam.ValidateLifetime,
                    RequireExpirationTime = JwtParam.RequireExpirationTime,
                    LifetimeValidator = (notBefore, expires, securityToken, validationParameters) =>
                    {
                        var TokenSet = securityToken as JwtSecurityToken;
                        var Exp = TokenSet.Claims.First(Item => Item.Type == "exp").Value;
                        if (long.TryParse(Exp, out var Time))
                        {
                            var ExpTime = DateTimeOffset.FromUnixTimeSeconds(Time).LocalDateTime;
                            return DateTime.Now < ExpTime;
                        }
                        return false;
                    }
                };
                var TokenHandler = new JwtSecurityTokenHandler();
                var TokenValidate = TokenHandler.ValidateToken(Token, TokenParam, out SecurityToken ValidatedToken);
                var DeToken = ValidatedToken as JwtSecurityToken;
                var GetClaims = DeToken?.Claims;
                if (GetClaims == null)
                    return false;

                User = new ClaimsPrincipal(new ClaimsIdentity(GetClaims));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }
        public bool IsBlackToken(string Token)
        {
            foreach (var Item in Setting.BlackTokenCheckList)
            {
                var IsBlack = Item.Invoke(Token, ServiceProvider);
                if (IsBlack)
                    return true;
            }
            return false;
        }
    }
}