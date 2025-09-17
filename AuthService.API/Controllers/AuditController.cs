using Microsoft.AspNetCore.Mvc;

namespace AgendaSalud.AuthService.API.Controllers
{
    public class AuditController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
