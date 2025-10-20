using Microsoft.AspNetCore.Mvc;

namespace ContractClaimSystem.Data
{
    public class ApplicationDbContext : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
