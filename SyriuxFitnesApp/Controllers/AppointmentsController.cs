using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SyriuxFitnesApp.Data;
using SyriuxFitnesApp.Models;

namespace SyriuxFitnesApp.Controllers
{
    [Authorize] // Sadece giriş yapmış üyeler erişebilir
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Appointments (Randevularım Listesi)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Sadece giriş yapan kullanıcıya ait randevuları getir
            var myAppointments = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Trainer)
                .Where(a => a.MemberId == user.Id)
                .OrderByDescending(a => a.AppointmentDate) // En yeni en üstte
                .ToListAsync();

            return View(myAppointments);
        }

        // GET: Appointments/Create
        public IActionResult Create()
        {
            // Başlangıçta tüm listeyi gönderiyoruz, filtrelemeyi JS yapacak.
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceName");
            // Trainer seçiminde isim soyisim görünsün
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

            // Otomatik alanları doldur
            appointment.MemberId = user.Id;
            appointment.IsApproved = false; // Varsayılan: Onay Bekliyor

            // --- KONTROL 1: GEÇMİŞ TARİH VE SAAT KONTROLÜ ---
            // Tarihi ve saati birleştirip tam şu an ile kıyaslıyoruz.
            if (!string.IsNullOrEmpty(appointment.AppointmentTime))
            {
                // Seçilen Tarih (Örn: 12.12.2025 00:00) + Seçilen Saat (Örn: 14:00) = 12.12.2025 14:00
                var combinedDateTime = appointment.AppointmentDate.Date.Add(TimeSpan.Parse(appointment.AppointmentTime));

                if (combinedDateTime < DateTime.Now)
                {
                    ModelState.AddModelError("", "Geçmiş bir tarih veya saate randevu alamazsınız.");
                }
            }
            else if (appointment.AppointmentDate < DateTime.Today)
            {
                // Eğer saat seçilmemişse (ki zorunlu ama yine de) sadece güne bak
                ModelState.AddModelError("", "Geçmiş bir tarihe randevu alamazsınız.");
            }

            // --- KONTROL 2: İLERİ TARİH SINIRI (Maksimum 30 Gün) ---
            // Bugünden itibaren 30 gün sonrasına kadar izin ver
            if (appointment.AppointmentDate > DateTime.Today.AddDays(30))
            {
                ModelState.AddModelError("", "En fazla 30 gün sonrasına randevu alabilirsiniz.");
            }
            // -------------------------------------------------------------

            if (ModelState.IsValid)
            {
                var selectedService = await _context.Services.FindAsync(appointment.ServiceId);
                var trainer = await _context.Trainers.FindAsync(appointment.TrainerId);

                // --- Hoca bu dersi veriyor mu kontrolü (Validasyon) ---
                if (appointment.TrainerId == 0 || appointment.ServiceId == 0)
                {
                    ModelState.AddModelError("", "HATA: Seçimler sisteme ulaşmadı. Lütfen sayfayı yenileyip tekrar deneyin.");
                }
                else
                {
                    bool validRelation = _context.TrainerServices.Any(ts => ts.TrainerId == appointment.TrainerId && ts.ServiceId == appointment.ServiceId);
                    if (!validRelation)
                    {
                        ModelState.AddModelError("", $"HATA: Seçilen antrenör (ID:{appointment.TrainerId}) bu hizmeti (ID:{appointment.ServiceId}) sistemde vermiyor görünüyor.");
                    }
                }
                // -------------------------------------------------------

                // Salon bilgisini çekiyoruz (Tek salon var varsayıyoruz)
                var salon = await _context.Salons.FirstOrDefaultAsync();

                if (selectedService != null && trainer != null && appointment.AppointmentTime != null)
                {
                    // Fiyat ve süreyi o anki tarifeden sabitle (İleride zam gelirse etkilenmesin)
                    appointment.StoredPrice = selectedService.Price;
                    appointment.StoredDuration = selectedService.DurationMinutes;

                    // Seçilen saati TimeSpan'e çevir
                    TimeSpan selectedTime = TimeSpan.Parse(appointment.AppointmentTime);
                    // Bitiş saatini hesapla (Başlangıç + Ders Süresi)
                    TimeSpan endTime = selectedTime.Add(TimeSpan.FromMinutes(selectedService.DurationMinutes));

                    // 1. Salon Saati Kontrolü 
                    if (salon != null)
                    {
                        if (selectedTime < salon.OpeningTime || endTime > salon.ClosingTime)
                        {
                            ModelState.AddModelError("", $"Salon çalışma saatleri ({salon.OpeningTime:hh\\:mm} - {salon.ClosingTime:hh\\:mm}) dışındasınız.");
                        }
                    }

                    // 2. Hoca Mesai Kontrolü
                    // Not: Eğer salon kontrolünden geçtiyse buraya bakarız
                    if (selectedTime < trainer.WorkStartHour || endTime > trainer.WorkEndHour)
                    {
                        ModelState.AddModelError("", $"Seçtiğiniz antrenör bu saatlerde çalışmıyor. (Mesai: {trainer.WorkStartHour:hh\\:mm} - {trainer.WorkEndHour:hh\\:mm})");
                    }

                    // 3. Antrenör Dolu mu? (Çakışma Kontrolü)
                    if (ModelState.IsValid)
                    {
                        var existingAppointments = await _context.Appointments
                            .Where(a => a.TrainerId == appointment.TrainerId && a.AppointmentDate == appointment.AppointmentDate)
                            .ToListAsync();

                        foreach (var existing in existingAppointments)
                        {
                            TimeSpan existingStart = TimeSpan.Parse(existing.AppointmentTime);
                            // Veritabanındaki randevunun süresini al
                            int duration = existing.StoredDuration > 0 ? existing.StoredDuration : selectedService.DurationMinutes;
                            TimeSpan existingEnd = existingStart.Add(TimeSpan.FromMinutes(duration));

                            // Çakışma Mantığı:
                            if (selectedTime < existingEnd && existingStart < endTime)
                            {
                                ModelState.AddModelError("", $"Seçilen saatte ({existing.AppointmentTime}) antrenörün başka bir randevusu var. Lütfen başka bir saat seçiniz.");
                                break;
                            }
                        }
                    }

                    // =================================================================================
                    // 4. KULLANICI (ÜYE) ÇAKIŞMA KONTROLÜ 
                    // =================================================================================
                    if (ModelState.IsValid)
                    {
                        // Kullanıcının o günkü kendi randevularını çekiyoruz
                        var myExistingAppointments = await _context.Appointments
                            .Include(a => a.Service) // Hizmet süresini öğrenmek için gerekli
                            .Where(a => a.MemberId == user.Id && a.AppointmentDate == appointment.AppointmentDate)
                            .ToListAsync();

                        foreach (var myApp in myExistingAppointments)
                        {
                            TimeSpan myStart = TimeSpan.Parse(myApp.AppointmentTime);
                            // Eğer veritabanında kayıtlı süre varsa onu al, yoksa servisin güncel süresini al
                            int myDuration = myApp.StoredDuration > 0 ? myApp.StoredDuration : (myApp.Service != null ? myApp.Service.DurationMinutes : 0);
                            TimeSpan myEnd = myStart.Add(TimeSpan.FromMinutes(myDuration));

                            // Çakışma Mantığı: (YeniStart < EskiEnd) VE (EskiStart < YeniEnd)
                            if (selectedTime < myEnd && myStart < endTime)
                            {
                                ModelState.AddModelError("", $"HATA: Seçtiğiniz saat aralığında ({myApp.AppointmentTime} - {myEnd:hh\\:mm}) zaten '{myApp.Service?.ServiceName}' randevunuz bulunmaktadır. Aynı anda iki randevuya katılamazsınız.");
                                break;
                            }
                        }
                    }
                    // =================================================================================
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Hata varsa formu tekrar doldurarak göster
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "ServiceName", appointment.ServiceId);
            ViewData["TrainerId"] = new SelectList(_context.Trainers, "TrainerId", "FullName", appointment.TrainerId);
            return View(appointment);
        }

        // POST: Appointments/Delete/5 (Sadece İptal Etme İşlemi)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------------------------
        // AJAX API METODLARI (Filtreleme ve Müsaitlik İçin)
        // ---------------------------------------------------------

        // 1. Antrenöre göre Servisleri getir
        [HttpGet]
        public JsonResult GetServicesByTrainer(int trainerId)
        {
            var services = _context.TrainerServices
                .Where(ts => ts.TrainerId == trainerId)
                .Include(ts => ts.Service)
                .Select(ts => new {
                    value = ts.ServiceId,
                    text = ts.Service.ServiceName,
                    duration = ts.Service.DurationMinutes,
                    price = ts.Service.Price
                })
                .ToList();

            return Json(services);
        }

        // 2. Servise göre Antrenörleri getir
        [HttpGet]
        public JsonResult GetTrainersByService(int serviceId)
        {
            var trainers = _context.TrainerServices
                .Where(ts => ts.ServiceId == serviceId)
                .Include(ts => ts.Trainer)
                .Select(ts => new {
                    value = ts.TrainerId,
                    text = ts.Trainer.FullName
                })
                .ToList();

            return Json(trainers);
        }

        // 3. Müsait Saatleri Getir  REST API KISMI)
        [HttpGet]
        public async Task<JsonResult> GetAvailableHours(int trainerId, int serviceId, DateTime date)
        {
            // Gerekli verileri çek
            var trainer = await _context.Trainers.FindAsync(trainerId);
            var service = await _context.Services.FindAsync(serviceId);
            var salon = await _context.Salons.FirstOrDefaultAsync();

            if (trainer == null || service == null || salon == null) return Json(new List<string>());

            // O günkü mevcut randevuları çek
            var appointments = await _context.Appointments
                .Where(a => a.TrainerId == trainerId && a.AppointmentDate == date)
                .ToListAsync();

            List<string> availableSlots = new List<string>();

            // Başlangıç ve Bitiş aralığını belirle (Hoca mesaisi ve Salon saatlerinin kesişimi)
            // Örn: Salon 09:00, Hoca 10:00 başlıyorsa -> Başlangıç 10:00
            TimeSpan start = (trainer.WorkStartHour > salon.OpeningTime) ? trainer.WorkStartHour : salon.OpeningTime;
            TimeSpan end = (trainer.WorkEndHour < salon.ClosingTime) ? trainer.WorkEndHour : salon.ClosingTime;

            // Döngü ile saatleri kontrol et (15'er dakika arayla)
            TimeSpan current = start;
            TimeSpan serviceDuration = TimeSpan.FromMinutes(service.DurationMinutes);

            while (current.Add(serviceDuration) <= end)
            {
                // Eğer tarih BUGÜN ise ve saat geçmişse, listeye ekleme
                if (date.Date == DateTime.Today && current < DateTime.Now.TimeOfDay)
                {
                    current = current.Add(TimeSpan.FromMinutes(15));
                    continue;
                }

                // Çakışma kontrolü
                bool isOccupied = false;
                TimeSpan potentialEnd = current.Add(serviceDuration);

                foreach (var app in appointments)
                {
                    TimeSpan appStart = TimeSpan.Parse(app.AppointmentTime);
                    // Veritabanında süre yoksa default servisten al (eski kayıtlar için)
                    int appDurationMinutes = app.StoredDuration > 0 ? app.StoredDuration : service.DurationMinutes;
                    TimeSpan appEnd = appStart.Add(TimeSpan.FromMinutes(appDurationMinutes));

                    // Kesişim formülü: (StartA < EndB) and (EndA > StartB)
                    if (current < appEnd && appStart < potentialEnd)
                    {
                        isOccupied = true;
                        break;
                    }
                }

                if (!isOccupied)
                {
                    // "hh:mm" formatında listeye ekle (Örn: 14:00)
                    availableSlots.Add(current.ToString(@"hh\:mm"));
                }

                // Bir sonraki bloğa geç (15 dk sonra)
                current = current.Add(TimeSpan.FromMinutes(15));
            }

            return Json(availableSlots);
        }
    }
}