using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyriuxFitnesApp.Data;
using SyriuxFitnesApp.Models;

namespace SyriuxFitnesApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SalonsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalonsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // TEK SAYFA: Ayarları Göster (GET)
        public async Task<IActionResult> Index()
        {
            // İlk kaydı getir
            var salon = await _context.Salons.FirstOrDefaultAsync();

            // Eğer veritabanı boşsa, varsayılan oluştur
            if (salon == null)
            {
                salon = new Salon
                {
                    SalonName = "Syriux Fitness",
                    OpeningTime = new TimeSpan(9, 0, 0),
                    ClosingTime = new TimeSpan(22, 0, 0)
                };
                _context.Salons.Add(salon);
                await _context.SaveChangesAsync();
            }

            return View(salon);
        }

        // AYARLARI GÜNCELLE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Salon salon)
        {
            if (ModelState.IsValid)
            {
                _context.Update(salon);
                await _context.SaveChangesAsync();

                // Başarı mesajı
                TempData["SuccessMessage"] = "Salon ayarları başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(salon);
        }
    }
}