using Microsoft.AspNetCore.Mvc.Filters;
using Rugal.TokenAuth.MVC.Service;

namespace Rugal.TokenAuth.MVC.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public partial class JwtAuthPageAttribute : Attribute, IAuthorizationFilter, IActionFilter
    {
        public JwtAuthPageAttribute() { }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var AuthPageService = context.HttpContext.RequestServices.GetService<AuthPageService>();
            AuthPageService.OnActionExecuted(context);
        }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var AuthPageService = context.HttpContext.RequestServices.GetService<AuthPageService>();
            AuthPageService.OnActionExecuting(context);
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var AuthPageService = context.HttpContext.RequestServices.GetService<AuthPageService>();
            AuthPageService.OnAuthorization(context);
        }
    }
}