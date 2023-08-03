using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Rugal.TokenAuth.Common.Interface;
using Rugal.TokenAuth.Common.Model;
using Rugal.TokenAuth.Common.Service;
using Rugal.TokenAuth.MVC.Model;

namespace Rugal.WebApi.Auth.JwtAuthorize
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class JwtAuthAttribute : Attribute, IAuthorizationFilter
    {
        public JwtAuthAttribute() : base()
        {

        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var HttpContext = context.HttpContext;
            var GetCookies = HttpContext.Request.Cookies;
            var GetToken = HttpContext.Request.Headers.Authorization.FirstOrDefault();

            if (GetToken is null && GetCookies.Any(Item => Item.Key == "Token"))
                GetToken = GetCookies.First(Item => Item.Key == "Token").Value;

            if (GetToken is null)
            {
                //context.Result = new JsonResult(AuthResultModel.Error("Token is null"));
                context.Result = new UnauthorizedResult();
                return;
            }

            GetToken = GetToken.Replace("bearer", "").Replace("Bearer", "").Trim();
            if (string.IsNullOrWhiteSpace(GetToken))
            {
                //context.Result = new JsonResult(AuthResultModel.Error("Token error"));
                context.Result = new UnauthorizedResult();
                return;
            }
            var GetTokenService = HttpContext.RequestServices.GetService<TokenService>();
            if (GetTokenService.IsBlackToken(GetToken))
            {
                //context.Result = new JsonResult(AuthResultModel.Error("Token logout"));
                context.Result = new UnauthorizedResult();
                return;
            }

            if (!GetTokenService.ValidateToken(GetToken, out var User))
            {
                //context.Result = new JsonResult(AuthResultModel.Error("Token validate error"));
                context.Result = new UnauthorizedResult();
                return;
            }

            HttpContext.User = User;

            var Setting = HttpContext.RequestServices.GetService<JwtAuthAttributeSetting>();
            var IsVerify = Setting.AuthVerify(context);
            if (!IsVerify)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var UserInfo = HttpContext.RequestServices.GetService<UserInfoModel>();
            var Configs = HttpContext.RequestServices.GetServices<IUserInfoConfig>();
            if (UserInfo is not null)
                UserInfo.Token = GetToken;

            foreach (var Config in Configs)
                Config.ConfigUserInfo(UserInfo);
        }
    }
}