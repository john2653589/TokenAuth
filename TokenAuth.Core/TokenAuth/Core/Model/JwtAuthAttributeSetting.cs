using Microsoft.AspNetCore.Mvc.Filters;

namespace Rugal.TokenAuth.MVC.Model
{
    public class JwtAuthAttributeSetting
    {
        public event Func<AuthorizationFilterContext, bool> OnAuthVerify;
        public bool AuthVerify(AuthorizationFilterContext Context)
        {
            var IsVerify = OnAuthVerify?.Invoke(Context) ?? true;
            return IsVerify;
        }
        public JwtAuthAttributeSetting Add_OnAuthVerify(Func<AuthorizationFilterContext, bool> OnAuthVerifyFunc)
        {
            OnAuthVerify += OnAuthVerifyFunc;
            return this;
        }
    }
}