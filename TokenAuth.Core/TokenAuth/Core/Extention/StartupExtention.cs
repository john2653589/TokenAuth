using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rugal.TokenAuth.Common.Interface;
using Rugal.TokenAuth.Common.Model;
using Rugal.TokenAuth.Common.Service;
using Rugal.TokenAuth.MVC.Model;

namespace Rugal.TokenAuth.Common.Extention
{
    public static class StartupExtention
    {
        public static IServiceCollection AddJwtParam(this IServiceCollection Services, ConfigurationManager Configuration)
        {
            var JwtParam = new JwtParamModel();
            Configuration.Bind("JwtParams", JwtParam);
            Services.AddSingleton(JwtParam);
            return Services;
        }
        public static IServiceCollection AddTokenAuth(this IServiceCollection Services, ConfigurationManager Configuration)
        {
            Services.AddTokenAuth_AttrSetting();
            Services.AddJwtParam(Configuration)
                .AddTokenService();

            return Services;
        }
        public static IServiceCollection AddTokenAuth_OnAuthVerify(this IServiceCollection Services, Func<AuthorizationFilterContext, bool> OnAuthVerifyFunc)
        {
            Services.AddTokenAuth_AttrSetting()
                .Add_OnAuthVerify(OnAuthVerifyFunc);
            return Services;
        }

        public static IServiceCollection AddTokenService(this IServiceCollection Services)
        {
            Services
                .AddScoped<TokenService>()
                .AddTokenAuth_Setting();
            return Services;
        }
        public static IServiceCollection AddTokenAuth_IsBlackToken(this IServiceCollection Services, Func<string, IServiceProvider, bool> IsBlackTokenCheck)
        {
            Services.AddTokenAuth_Setting()
                .AddBlackTokenCheck(IsBlackTokenCheck);
            return Services;
        }
        public static IServiceCollection AddUserInfo(this IServiceCollection Services)
        {
            Services.AddHttpContextAccessor();
            Services.AddScoped(ItemService =>
            {
                var Context = ItemService.GetService<IHttpContextAccessor>();
                var Claims = Context.HttpContext.User.Claims;
                var GetUserInfo = new UserInfoModel()
                {
                    Claims = Claims.ToList(),
                };
                return GetUserInfo;
            });
            return Services;
        }
        public static IServiceCollection AddUserInfo_Config<IConfig>(this IServiceCollection Services) where IConfig : class, IUserInfoConfig
        {
            Services.AddScoped<IUserInfoConfig, IConfig>();
            return Services;
        }
        public static IServiceCollection AddTokenAuth_PasswordHasher<IHash>(this IServiceCollection Services)
            where IHash : class, IPasswordHasher<IdentityUser>
        {
            Services.AddScoped<IPasswordHasher<IdentityUser>, IHash>();
            return Services;
        }

        private static TokenAuthSetting AddTokenAuth_Setting(this IServiceCollection Services)
        {

            var TokenSetting = Services.BuildServiceProvider().GetService<TokenAuthSetting>();
            if (TokenSetting is null)
            {
                TokenSetting = new TokenAuthSetting();
                Services.AddSingleton(TokenSetting);
            }
            return TokenSetting;
        }
        private static JwtAuthAttributeSetting AddTokenAuth_AttrSetting(this IServiceCollection Services)
        {
            var AttrSetting = Services.BuildServiceProvider().GetService<JwtAuthAttributeSetting>();
            if (AttrSetting is null)
            {
                AttrSetting = new JwtAuthAttributeSetting();
                Services.AddSingleton(AttrSetting);
            }
            return AttrSetting;
        }
    }
}