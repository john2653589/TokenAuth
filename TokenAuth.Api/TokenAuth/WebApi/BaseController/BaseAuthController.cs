using Microsoft.AspNetCore.Mvc;
using Rugal.TokenAuth.WebApi.Model;
using Rugal.TokenAuth.WebApi.Service;

namespace Rugal.TokenAuth.WebApi.BaseController
{
    public abstract class BaseAuthController : ControllerBase
    {
        private readonly BaseAuthService BaseAuthService;
        protected BaseAuthController(BaseAuthService _BaseAuthService)
        {
            BaseAuthService = _BaseAuthService;
        }

        [HttpPost]
        internal virtual async Task<dynamic> BaseSignIn(BaseSignInModel Model) => await BaseAuthService.SignIn(Model);

        [HttpPost]
        internal virtual async Task<dynamic> BaseRegister(BaseRegisterModel Model) => await BaseAuthService.Register(Model);
    }
}