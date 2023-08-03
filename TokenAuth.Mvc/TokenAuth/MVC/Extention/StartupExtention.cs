using Rugal.TokenAuth.MVC.Model;
using Rugal.TokenAuth.MVC.Service;

namespace Rugal.TokenAuth.MVC.Extention
{
    public static class StartupExtention
    {
        public static IServiceCollection AddAuthPage(this IServiceCollection Services)
        {
            Services.AddScoped<AuthPageService>();
            return Services;
        }

        public static IServiceCollection AddAuthPage(this IServiceCollection Services, Action<AuthPageService> AuthPageFunc)
        {
            Services.AddScoped(Provider =>
            {
                var AuthPageService = new AuthPageService();
                AuthPageFunc.Invoke(AuthPageService);
                return AuthPageService;
            });
            return Services;
        }

        public static IServiceCollection AddTokenAuth_RouterSetting(this IServiceCollection Services)
        {
            Services.AddTokenAuth_AttrRouterSetting();
            return Services;
        }
        public static IServiceCollection AddTokenAuth_RouterSetting(this IServiceCollection Services, Action<JwtAuthAttributeRouterSetting> SettingFunc)
        {
            var Setting = Services.AddTokenAuth_AttrRouterSetting();
            SettingFunc.Invoke(Setting);
            return Services;
        }
        private static JwtAuthAttributeRouterSetting AddTokenAuth_AttrRouterSetting(this IServiceCollection Services)
        {
            var AttrSetting = Services.BuildServiceProvider().GetService<JwtAuthAttributeRouterSetting>();
            if (AttrSetting is null)
            {
                AttrSetting = new JwtAuthAttributeRouterSetting();
                Services.AddSingleton(AttrSetting);
            }
            return AttrSetting;
        }
    }
}