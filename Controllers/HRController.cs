using Microsoft.AspNetCore.Mvc;

namespace ContractClaimSystem.Controllers
{
    public class HRController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
