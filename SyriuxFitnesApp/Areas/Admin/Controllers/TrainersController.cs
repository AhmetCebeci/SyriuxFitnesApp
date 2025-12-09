using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SyriuxFitnesApp.Data;
using SyriuxFitnesApp.Models;

namespace SyriuxFitnesApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TrainersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Trainers
        public async Task<IActionResult> Index()
        {
            // Include ekledik ki hizmetleri görelim
            var trainers = await _context.Trainers
                .Include(t => t.TrainerServices)
                .ThenInclude(ts => ts.Service)
                .ToListAsync();
            return View(trainers);
        }

        // GET: Admin/Trainers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.TrainerServices).ThenInclude(ts => ts.Service)
                .FirstOrDefaultAsync(m => m.TrainerId == id);
            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }

        // GET: Admin/Trainers/Create
        public IActionResult Create()
        {
            // Checkbox listesi için
            ViewBag.Services = _context.Services.ToList();
            return View();
        }

        // POST: Admin/Trainers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TrainerId,FullName,Expertise,WorkStartHour,WorkEndHour")] Trainer trainer, int[] selectedServices)
        {
            // --- EKLEME: Salon Saati Kontrolü ---
            var salon = _context.Salons.FirstOrDefault();
            if (salon != null)
            {
                int salonAcilis = salon.OpeningTime.Hours;
                int salonKapanis = salon.ClosingTime.Hours;
                if (trainer.WorkStartHour < salonAcilis || trainer.WorkEndHour > salonKapanis)
                {
                    ModelState.AddModelError("", $"Hata: Hoca çalışma saatleri, Salon saatleri ({salonAcilis}:00 - {salonKapanis}:00) arasında olmalıdır.");
                }
                if (trainer.WorkStartHour >= trainer.WorkEndHour)
                {
                    ModelState.AddModelError("", "Hata: Başlangıç saati, bitiş saatinden önce olmalıdır.");
                }
            }
            // ------------------------------------

            if (ModelState.IsValid)
            {
                _context.Add(trainer);
                await _context.SaveChangesAsync();

                // --- EKLEME: Servisleri Kaydetme ---
                if (selectedServices != null && selectedServices.Length > 0)
                {
                    foreach (var serviceId in selectedServices)
                    {
                        var trainerService = new TrainerService
                        {
                            TrainerId = trainer.TrainerId,
                            ServiceId = serviceId
                        };
                        _context.TrainerServices.Add(trainerService);
                    }
                    await _context.SaveChangesAsync();
                }
                // -----------------------------------

                return RedirectToAction(nameof(Index));
            }
            ViewBag.Services = _context.Services.ToList();
            return View(trainer);
        }

        // GET: Admin/Trainers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.TrainerServices)
                .FirstOrDefaultAsync(x => x.TrainerId == id);

            if (trainer == null)
            {
                return NotFound();
            }

            ViewBag.Services = _context.Services.ToList();
            ViewBag.SelectedServiceIds = trainer.TrainerServices.Select(ts => ts.ServiceId).ToList();

            return View(trainer);
        }

        // POST: Admin/Trainers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TrainerId,FullName,Expertise,WorkStartHour,WorkEndHour")] Trainer trainer, int[] selectedServices)
        {
            if (id != trainer.TrainerId)
            {
                return NotFound();
            }

            // --- EKLEME: Salon Saati Kontrolü ---
            var salon = _context.Salons.FirstOrDefault();
            if (salon != null)
            {
                int salonAcilis = salon.OpeningTime.Hours;
                int salonKapanis = salon.ClosingTime.Hours;
                if (trainer.WorkStartHour < salonAcilis || trainer.WorkEndHour > salonKapanis)
                {
                    ModelState.AddModelError("", $"Hata: Çalışma saatleri salon saatlerine ({salonAcilis}:00 - {salonKapanis}:00) uymuyor.");
                }
                if (trainer.WorkStartHour >= trainer.WorkEndHour)
                {
                    ModelState.AddModelError("", "Hata: Başlangıç saati geçersiz.");
                }
            }
            // ------------------------------------

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainer);
                    await _context.SaveChangesAsync();

                    // --- EKLEME: Servisleri Güncelleme ---
                    var existingServices = _context.TrainerServices.Where(ts => ts.TrainerId == id);
                    _context.TrainerServices.RemoveRange(existingServices);

                    if (selectedServices != null)
                    {
                        foreach (var serviceId in selectedServices)
                        {
                            _context.TrainerServices.Add(new TrainerService { TrainerId = id, ServiceId = serviceId });
                        }
                    }
                    await _context.SaveChangesAsync();
                    // -------------------------------------
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainerExists(trainer.TrainerId))
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
            ViewBag.Services = _context.Services.ToList();
            ViewBag.SelectedServiceIds = selectedServices.ToList();
            return View(trainer);
        }

        // GET: Admin/Trainers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.Appointments)
                .Include(t => t.TrainerServices).ThenInclude(ts => ts.Service)
                .FirstOrDefaultAsync(m => m.TrainerId == id);

            if (trainer == null)
            {
                return NotFound();
            }

            // Randevu sayısını View'a taşı
            ViewBag.AppointmentCount = trainer.Appointments != null ? trainer.Appointments.Count : 0;

            return View(trainer);
        }

        // POST: Admin/Trainers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainer = await _context.Trainers
                .Include(t => t.Appointments)
                .FirstOrDefaultAsync(x => x.TrainerId == id);

            if (trainer != null)
            {
                // Randevuları sil
                if (trainer.Appointments != null && trainer.Appointments.Any())
                {
                    _context.Appointments.RemoveRange(trainer.Appointments);
                }

                _context.Trainers.Remove(trainer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TrainerExists(int id)
        {
            return _context.Trainers.Any(e => e.TrainerId == id);
        }
    }
}