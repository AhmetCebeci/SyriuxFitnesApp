using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SyriuxFitnesApp.Data;
using SyriuxFitnesApp.Models;

namespace SyriuxFitnesApp.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Appointments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Appointments.Include(a => a.Member).Include(a => a.Service).Include(a => a.Trainer);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointments/Create
        public IActionResult Create()
        {
            ViewData["MemberId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceName");
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "TrainerId", "Expertise");
            return View();
        }



















        // POST: Appointments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppointmentId,AppointmentDate,AppointmentTime,TrainerId,ServiceId")] Appointment appointment)
        {
            // 1. Giriş yapan üyeyi bul (Identity)
            var userEmail = User.Identity?.Name;
            if (userEmail != null)
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user != null) appointment.MemberId = user.Id;
            }

            // --- 2. GEÇMİŞ TARİH KONTROLÜ (İLK ADIM) ---
            // Eğer tarih eskiyese geçmiş bir randevu alınamaz,hata ver
            if (appointment.AppointmentDate < DateTime.Today)
            {
                ModelState.AddModelError("", "Geçmiş bir tarihe randevu alamazsınız.");
            }

            // Tarih geçerliyse diğer kontrolleri yap
            if (ModelState.IsValid)
            {
                // Seçilen Hizmetin Süresini ve Hocayı Bul
                var selectedService = _context.Services.Find(appointment.ServiceId);
                var trainer = _context.Trainers.Find(appointment.TrainerId);

                if (selectedService != null && trainer != null && appointment.AppointmentTime != null)
                {
                    // --- A) ZAMAN HESAPLAMALARI ---
                    TimeSpan timeSpan = TimeSpan.Parse(appointment.AppointmentTime);
                    DateTime newStart = appointment.AppointmentDate.Date + timeSpan;
                    DateTime newEnd = newStart.AddMinutes(selectedService.DurationMinutes);

                    // --- B) MESAİ KONTROLÜ ---
                    int startHour = timeSpan.Hours;
                    int endHour = newEnd.Hour;
                    if (newEnd.Minute > 0) endHour++;

                    if (startHour < trainer.WorkStartHour || endHour > trainer.WorkEndHour)
                    {
                        ModelState.AddModelError("", $"Antrenörün mesai saatleri: {trainer.WorkStartHour}:00 - {trainer.WorkEndHour}:00. İşlem bu saatleri aşıyor.");
                    }

                    // --- C) ÇAKIŞMA KONTROLÜ  ---
                    // O hocanın o günkü randevularını çek
                    var existingAppointments = _context.Appointments
                        .Include(a => a.Service)
                        .Where(a => a.TrainerId == appointment.TrainerId && a.AppointmentDate == appointment.AppointmentDate)
                        .ToList();

                    foreach (var existing in existingAppointments)
                    {
                        TimeSpan existingTime = TimeSpan.Parse(existing.AppointmentTime);
                        DateTime existingStart = existing.AppointmentDate.Date + existingTime;
                        DateTime existingEnd = existingStart.AddMinutes(existing.Service.DurationMinutes);

                        // Aralık Çakışması Kontrolü
                        if (newStart < existingEnd && existingStart < newEnd)
                        {
                            ModelState.AddModelError("", $"Seçilen saatlerde ({existing.AppointmentTime}) antrenör dolu. İşlem süreniz ({selectedService.DurationMinutes} dk) nedeniyle çakışma oluyor.");
                            break;
                        }
                    }
                }
            }

            // Tüm kontrollerden geçtiyse KAYDET
            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Hata varsa sayfayı tekrar yükle (Dropdownları doldur)
            ViewData["MemberId"] = new SelectList(_context.Users, "Id", "Id", appointment.MemberId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceName", appointment.ServiceId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "TrainerId", "FullName", appointment.TrainerId);
            return View(appointment);
        }













        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            ViewData["MemberId"] = new SelectList(_context.Users, "Id", "Id", appointment.MemberId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceName", appointment.ServiceId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "TrainerId", "Expertise", appointment.TrainerId);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AppointmentId,AppointmentDate,AppointmentTime,IsApproved,MemberId,TrainerId,ServiceId")] Appointment appointment)
        {
            if (id != appointment.AppointmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.AppointmentId))
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
            ViewData["MemberId"] = new SelectList(_context.Users, "Id", "Id", appointment.MemberId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceName", appointment.ServiceId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "TrainerId", "Expertise", appointment.TrainerId);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.AppointmentId == id);
        }
    }
}
