using Microsoft.AspNetCore.Mvc;
using Rugal.TokenAuth.MVC.Attributes;

namespace Rugal.TokenAuth.MVC.Controllers
{
    [JwtAuthPage]
    public abstract class AuthController : Controller
    {
        public AuthController() { }
    }
}