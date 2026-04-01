using Microsoft.AspNetCore.Mvc;

namespace OidcServer.Controllers
{
    public class AuthorizeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
