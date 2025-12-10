using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SyriuxFitnesApp.Data;
using SyriuxFitnesApp.Models;

namespace SyriuxFitnesApp.Controllers
{
    [Authorize] // Sadece giriş yapmış üyeler
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Appointments (Randevularım)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var myAppointments = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .Where(a => a.MemberId == user.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(myAppointments);
        }

        // GET: Appointments/Create
        public IActionResult Create()
        {
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceName");
            // Trainer Expertise yerine FullName gösterilsin
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "TrainerId", "FullName");
            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppointmentDate,AppointmentTime,TrainerId,ServiceId")] Appointment appointment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            appointment.MemberId = user.Id;
            appointment.IsApproved = false;

            // --- GEÇMİŞ TARİH KONTROLÜ ---
            if (appointment.AppointmentDate < DateTime.Today)
            {
                ModelState.AddModelError("", "Geçmiş bir tarihe randevu alamazsınız.");
            }

            if (ModelState.IsValid)
            {
                var selectedService = await _context.Services.FindAsync(appointment.ServiceId);
                var trainer = await _context.Trainers.FindAsync(appointment.TrainerId);

                if (selectedService != null && trainer != null && appointment.AppointmentTime != null)
                {
                    // --- FİYAT VE SÜRE SABİTLEME ---
                    appointment.StoredPrice = selectedService.Price;
                    appointment.StoredDuration = selectedService.DurationMinutes;

                    // --- MESAİ KONTROLÜ (TimeSpan Uyumlu) ---
                    TimeSpan selectedTime = TimeSpan.Parse(appointment.AppointmentTime);
                    TimeSpan endTime = selectedTime.Add(TimeSpan.FromMinutes(selectedService.DurationMinutes));

                    if (selectedTime < trainer.WorkStartHour || endTime > trainer.WorkEndHour)
                    {
                        ModelState.AddModelError("", $"Antrenörün mesai saatleri: {trainer.WorkStartHour:hh\\:mm} - {trainer.WorkEndHour:hh\\:mm}. Seçilen saat bu aralık dışında.");
                    }

                    // --- ÇAKIŞMA KONTROLÜ ---
                    if (ModelState.IsValid)
                    {
                        var existingAppointments = await _context.Appointments
                            .Where(a => a.TrainerId == appointment.TrainerId && a.AppointmentDate == appointment.AppointmentDate)
                            .ToListAsync();

                        foreach (var existing in existingAppointments)
                        {
                            TimeSpan existingStart = TimeSpan.Parse(existing.AppointmentTime);
                            int duration = existing.StoredDuration > 0 ? existing.StoredDuration : selectedService.DurationMinutes;
                            TimeSpan existingEnd = existingStart.Add(TimeSpan.FromMinutes(duration));

                            if (selectedTime < existingEnd && existingStart < endTime)
                            {
                                ModelState.AddModelError("", $"Seçilen saatlerde ({existing.AppointmentTime}) antrenör dolu.");
                                break;
                            }
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceName", appointment.ServiceId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "TrainerId", "FullName", appointment.TrainerId);
            return View(appointment);
        }
    }
}