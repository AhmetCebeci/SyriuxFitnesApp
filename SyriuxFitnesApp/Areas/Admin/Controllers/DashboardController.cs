using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SyriuxFitnesApp.Areas.Admin.Controllers
{
    // 1. Bu controller'ın "Admin" alanına ait olduğunu belirtiyoruz.
    [Area("Admin")]

    // 2. Sadece "Admin" rolüne sahip kullanıcıların girebileceğini söylüyoruz.
    [Authorize(Roles = "Admin")]
    public class DashboardController:Controller
    {
        public IActionResult Index()
        {
            // İleride burada istatistikleri (üye sayısı, randevu sayısı) veritabanından çekeceğiz.
            // Şimdilik sadece boş sayfayı döndürüyoruz.
            return View();
        }
    }
}
