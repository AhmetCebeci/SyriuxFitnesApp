using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SyriuxFitnesApp.Data;
using SyriuxFitnesApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SyriuxFitnesApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Members (Üye Listesi)
        public async Task<IActionResult> Index()
        {
            //  Admin Rolündekileri Gizle ---

            // 1. Önce "Admin" rolünün ID'sini buluyoruz
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");

            if (adminRole != null)
            {
                // 2. Bu Role ID'sine sahip kullanıcıların (Adminlerin) ID'lerini listeliyoruz
                var adminUserIds = await _context.UserRoles
                    .Where(ur => ur.RoleId == adminRole.Id)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                // 3. Kullanıcılar tablosundan, ID'si admin listesinde OLMAYANLARI (!Contains) çekiyoruz
                var normalUsers = await _context.Users
                    .Where(u => !adminUserIds.Contains(u.Id))
                    .ToListAsync();

                return View(normalUsers);
            }

            // Eğer sistemde Admin rolü yoksa mecburen hepsini getir (Hata olmasın diye)
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // GET: Admin/Members/Delete/5 (Silme Onay Ekranı)
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            // Kullanıcıyı ve ilişkili randevularını buluyoruz (Silmeden önce uyarmak için)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id); // AppUser'da Id string türündedir (Guid)

            if (user == null) return NotFound();

            // Bu üyenin kaç randevusu var? (Veritabanında Appointment tablosunda MemberId ile eşleşenler)
            var randevuSayisi = await _context.Appointments.CountAsync(a => a.MemberId == id);
            ViewBag.RandevuSayisi = randevuSayisi;

            return View(user);
        }

        // POST: Admin/Members/Delete/5 (Kesin Silme İşlemi)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user != null)
            {
                // İlişkili randevuları bul ve sil (User silinince FK hatası almamak için)
                var userAppointments = _context.Appointments.Where(a => a.MemberId == id);
                _context.Appointments.RemoveRange(userAppointments);

                // Kullanıcıyı sil
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}