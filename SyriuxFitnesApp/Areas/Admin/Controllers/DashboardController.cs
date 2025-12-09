using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyriuxFitnesApp.Data; // Veritabanı için eklendi
using System.Linq; // Count() işlemleri için eklendi

namespace SyriuxFitnesApp.Areas.Admin.Controllers
{
    // 1. Bu controller'ın "Admin" alanına ait olduğunu belirtiyoruz.
    [Area("Admin")]

    // 2. Sadece "Admin" rolüne sahip kullanıcıların girebileceğini söylüyoruz.
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        // --- EKLEME BAŞLANGIÇ: Veritabanı bağlantısı ---
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        // --- EKLEME BİTİŞ ---

        public IActionResult Index()
        {
            // İleride burada istatistikleri (üye sayısı, randevu sayısı) veritabanından çekeceğiz.
            // Şimdilik sadece boş sayfayı döndürüyoruz.

            // --- Çekme işlemini şimdi yapıyoruz ---
            // Admin de Users tablosunda olduğu için, onu saymamak adına toplamdan 1 çıkarıyoruz.
            ViewBag.UyeSayisi = _context.Users.Count()-1;
            ViewBag.AntrenorSayisi = _context.Trainers.Count();

            ViewBag.BekleyenRandevu = _context.Appointments.Where(x => x.IsApproved == false).Count();
            // --------------------------------------

            return View();
        }
    }
}