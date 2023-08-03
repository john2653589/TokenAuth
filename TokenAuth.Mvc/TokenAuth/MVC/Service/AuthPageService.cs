using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Rugal.NetCommon.Extention.Expando;
using Rugal.TokenAuth.Common.Interface;
using Rugal.TokenAuth.Common.Model;
using Rugal.TokenAuth.Common.Service;
using Rugal.TokenAuth.MVC.Model;
using System.Dynamic;
using System.Security.Claims;
using Rugal.MvcCommon.Extention;

namespace Rugal.TokenAuth.MVC.Service
{
    public class AuthPageService
    {
        public Dictionary<string, object> AddData { get; set; }
        public event Action<Controller> OnActionBefore;
        public event Action<Controller> OnActionAfter;
        private UserInfoModel UserInfo;
        public AuthPageService()
        {
            AddData = new Dictionary<string, object>();
        }
        public void OnAuthorization(AuthorizationFilterContext Context)
        {
            var HttpContext = Context.HttpContext;

            var Cookies = HttpContext.Request.Cookies;

            var RouterSetting = HttpContext.RequestServices.GetService<JwtAuthAttributeRouterSetting>();
            var TokenKey = RouterSetting.CookieTokenKey;

            var GetToken = Cookies[TokenKey];

            var ReturnCookie = HttpContext.Response.Cookies;
            if (GetToken != null)
            {
                ReturnCookie.Delete(TokenKey);
                ReturnCookie.Append(TokenKey, GetToken, new CookieOptions()
                {
                    HttpOnly = true,
                    Secure = false,
                });
            }

            IActionResult UnAuthResult = RouterSetting?.UnAuthType switch
            {
                UnAuthRedirectType.Url => new RedirectResult(RouterSetting.UnAuthUrl),
                UnAuthRedirectType.Route => new RedirectToRouteResult(RouterSetting.UnAuthRoute),
                _ => new RedirectToRouteResult(RouterSetting.UnAuthRoute),
            };

            var GetTokenService = HttpContext.RequestServices.GetService<TokenService>();
            if (string.IsNullOrWhiteSpace(GetToken) || !GetTokenService.ValidateToken(GetToken, out var User))
            {
                ReturnCookie.Delete(TokenKey);
                Context.Result = UnAuthResult;
                return;
            }

            HttpContext.User = User;

            var Setting = HttpContext.RequestServices.GetService<JwtAuthAttributeSetting>();
            var IsVerify = Setting.AuthVerify(Context);
            if (!IsVerify)
            {
                ReturnCookie.Delete(TokenKey);
                Context.Result = UnAuthResult;
                return;
            }

            var UserInfo = HttpContext.RequestServices.GetService<UserInfoModel>();
            var Configs = HttpContext.RequestServices.GetServices<IUserInfoConfig>();
            if (UserInfo is not null)
                UserInfo.Token = GetToken;

            foreach (var Config in Configs)
                Config.ConfigUserInfo(UserInfo);
        }
        public void OnActionExecuting(ActionExecutingContext Context)
        {
            var HttpContext = Context.HttpContext;
            UserInfo = HttpContext.RequestServices.GetService<UserInfoModel>();
            OnActionBefore?.Invoke(Context.Controller as Controller);
        }
        public void OnActionExecuted(ActionExecutedContext Context)
        {
            var Rotuer = Context.Controller as Controller;
            if (Rotuer.ViewBag.DefaultModel is null)
                Rotuer.ViewBag.DefaultModel = new ExpandoObject();

            Rotuer.ViewBag.DefaultModel.UserInfo = UserInfo;
            Rotuer.ViewBag.DefaultModel.Token = UserInfo.Token;

            var DefaultModelObject = Rotuer.ViewBag.DefaultModel as ExpandoObject;
            DefaultModelObject.Extend(AddData);

            var DefaultData = DefaultModelObject.ToHtmlString();
            Rotuer.ViewBag.DefaultData = DefaultData;
            OnActionAfter?.Invoke(Context.Controller as Controller);
        }
        public bool AddDefaultData(string Key, object Value)
        {
            if (AddData.ContainsKey(Key))
            {
                AddData[Key] = Value;
                return true;
            }
            return AddData.TryAdd(Key, Value);
        }
        public bool AddDefaultData(object Model)
        {
            if (Model is null)
                return false;

            var GetModel = Model.ToExpando();
            foreach (var Item in GetModel)
                AddDefaultData(Item.Key, Item.Value);
            return true;
        }
        public void AddUserInfo(object Model)
        {
            if (Model is null)
                return;

            var GetModel = Model.ToExpando();
            foreach (var Item in GetModel)
            {
                if (Item.Value is int IntValue)
                    AddUserInfo(Item.Key, IntValue);
                else if (Item.Value is bool BoolValue)
                    AddUserInfo(Item.Key, BoolValue);
                else
                    AddUserInfo(Item.Key, Item.Value);
            }
        }
        public void AddUserInfo(string Key, int Value)
        {
            AddUserInfo(Key, Value, ClaimValueTypes.Integer);
        }
        public void AddUserInfo(string Key, bool Value)
        {
            AddUserInfo(Key, Value, ClaimValueTypes.Boolean);
        }
        public void AddUserInfo(string Key, object Value, string ValueType = ClaimValueTypes.String)
        {
            var AddClaim = new Claim(Key, Value.ToString(), ValueType);
            UserInfo.Claims.Add(AddClaim);
        }
    }
}