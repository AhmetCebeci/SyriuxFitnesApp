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
        // --- Veritabanı bağlantısı ---
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        // -----------------------------
        public IActionResult Index()
        {
            // --- Çekme işlemini yapıyoruz ---

            // Dinamik Admin Sayısı Düşme ---
            // Admin de Users tablosunda olduğu için, onu saymamak adına admin rolündeki kişi sayısını bulup düşüyoruz.
            var adminRoleId = _context.Roles.FirstOrDefault(r => r.Name == "Admin")?.Id;
            var adminCount = _context.UserRoles.Count(ur => ur.RoleId == adminRoleId);

            ViewBag.UyeSayisi = _context.Users.Count() - adminCount;
            // ------------------------------

            ViewBag.AntrenorSayisi = _context.Trainers.Count();

            // Bekleyen ve Onaylanan Randevular
            ViewBag.BekleyenRandevu = _context.Appointments.Where(x => x.IsApproved == false).Count();
            ViewBag.OnaylananRandevu = _context.Appointments.Where(x => x.IsApproved == true).Count();

            // Servis Sayısı
            ViewBag.ServisSayisi = _context.Services.Count();

            // Salon Bilgileri (Tek bir salon olduğu varsayımıyla ilk kaydı alıyoruz)
            var salon = _context.Salons.FirstOrDefault();
            if (salon != null)
            {
                ViewBag.SalonAdi = salon.SalonName;
                // Saat formatını (09:00 - 22:00) şeklinde ayarlıyoruz
                ViewBag.CalismaSaatleri = $"{salon.OpeningTime:hh\\:mm} - {salon.ClosingTime:hh\\:mm}";
            }
            else
            {
                ViewBag.SalonAdi = "Salon Tanımlı Değil";
                ViewBag.CalismaSaatleri = "-";
            }
            // --------------------------------

            return View();
        }
    }
}