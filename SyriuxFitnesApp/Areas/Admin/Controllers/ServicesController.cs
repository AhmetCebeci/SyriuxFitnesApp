using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // Admin yetkisi için
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SyriuxFitnesApp.Data;
using SyriuxFitnesApp.Models;

namespace SyriuxFitnesApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Sadece Adminler girebilsin
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Services
        public async Task<IActionResult> Index()
        {
            return View(await _context.Services.ToListAsync());
        }

        // GET: Admin/Services/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .FirstOrDefaultAsync(m => m.ServiceId == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // GET: Admin/Services/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Services/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ServiceId,ServiceName,DurationMinutes,Price")] Service service)
        {
            if (ModelState.IsValid)
            {
                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        // GET: Admin/Services/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        // POST: Admin/Services/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceId,ServiceName,DurationMinutes,Price")] Service service)
        {
            if (id != service.ServiceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.ServiceId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        // GET: Admin/Services/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // İlişkili randevuları (Appointments) da çekiyoruz ki sayısını görelim.
            // Bu hizmeti veren hocaları (TrainerServices) ve İsimlerini (Trainer) çekiyoruz.
            var service = await _context.Services
                .Include(s => s.Appointments)
                .Include(s => s.TrainerServices) // <--  Hoca sayısı için
                    .ThenInclude(ts => ts.Trainer) // <--  Hoca isimlerini çekmek için
                .FirstOrDefaultAsync(m => m.ServiceId == id);

            if (service == null)
            {
                return NotFound();
            }

            // View'a gönderirken randevu sayısını ViewBag ile taşıyalım
            ViewBag.AppointmentCount = service.Appointments != null ? service.Appointments.Count : 0;

            // View'a Hoca sayısını da gönderiyoruz
            ViewBag.TrainerCount = service.TrainerServices != null ? service.TrainerServices.Count : 0;

            // View'a Hoca İsimlerini Liste olarak gönderiyoruz
            ViewBag.TrainerNames = service.TrainerServices != null
                ? service.TrainerServices.Select(ts => ts.Trainer.FullName).ToList()
                : new List<string>();

            return View(service);
        }

        // POST: Admin/Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // İlişkili verilerle beraber çekiyoruz (özellikle TrainerServices otomatik silinir ama Appointments sorun olabilir)
            var service = await _context.Services
                .Include(s => s.Appointments)
                .Include(s => s.TrainerServices) // Hoca bağlantılarını da çekiyoruz
                .FirstOrDefaultAsync(x => x.ServiceId == id);

            if (service != null)
            {
                // 1. Önce bu servise ait randevuları silelim (veya null yapabiliriz ama silmek daha temiz)
                if (service.Appointments != null && service.Appointments.Count > 0)
                {
                    _context.Appointments.RemoveRange(service.Appointments);
                }

                // 2. Servisi sil (TrainerServices EF Core tarafından otomatik silinir genelde cascade ise, değilse burası da siler)
                _context.Services.Remove(service);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ServiceId == id);
        }
    }
}