using Microsoft.AspNetCore.Mvc.Filters;

namespace Rugal.TokenAuth.MVC.Model
{
    public class JwtAuthAttributeRouterSetting
    {
        private Dictionary<string, object> SetUnAuthRoute { get; set; }
        public string CookieTokenKey { get; set; } = "Token";
        public UnAuthRedirectType UnAuthType { get; set; } = UnAuthRedirectType.Route;
        public RouteValueDictionary UnAuthRoute => GetUnAuthRoute();
        public string UnAuthUrl { get; set; } = "/";
        public JwtAuthAttributeRouterSetting WithUnAuthRedirect(Dictionary<string, object> _RouteValue)
        {
            UnAuthType = UnAuthRedirectType.Route;
            SetUnAuthRoute = _RouteValue;
            return this;
        }
        public JwtAuthAttributeRouterSetting WithUnAuthUrl(string _UnAuthUrl)
        {
            UnAuthUrl = _UnAuthUrl;
            UnAuthType = UnAuthRedirectType.Url;
            return this;
        }
        public JwtAuthAttributeRouterSetting WithCookieTokenKey(string TokenKey)
        {
            CookieTokenKey = TokenKey;
            return this;
        }
        private RouteValueDictionary GetUnAuthRoute()
        {
            SetUnAuthRoute ??= new Dictionary<string, object>
            {
                { "controller", "User" },
                { "action", "Login" }
            };
            var Route = new RouteValueDictionary(SetUnAuthRoute);
            return Route;
        }
    }

    public enum UnAuthRedirectType
    {
        Route,
        Url
    }
}