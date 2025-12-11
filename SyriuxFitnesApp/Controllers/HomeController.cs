using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SyriuxFitnesApp.Models;

namespace SyriuxFitnesApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            //  Admin Yönlendirmesi --
            // Eðer kullanýcý giriþ yapmýþsa (User.Identity.IsAuthenticated)
            // VE Rolü "Admin" ise, onu Ana Sayfa yerine Dashboard'a atýyoruz.
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Dashboard", new { Area = "Admin" });
            }
            // ----------------------

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}