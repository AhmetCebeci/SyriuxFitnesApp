// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; // CountAsync için gerekli
using SyriuxFitnesApp.Data; // DbContext için
using SyriuxFitnesApp.Models;

namespace SyriuxFitnesApp.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _context; // Randevuları kontrol etmek için

        public IndexModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        // Silme işlemi için şifre alanı (Modal içinden gelecek)
        [BindProperty]
        public string DeletePassword { get; set; }

        // Kullanıcının kaç randevusu olduğunu ekranda göstermek için
        public int ActiveAppointmentCount { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Ad alanı zorunludur.")]
            [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad 2-50 karakter arasında olmalıdır.")]
            [Display(Name = "Ad")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Soyad alanı zorunludur.")]
            [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad 2-50 karakter arasında olmalıdır.")]
            [Display(Name = "Soyad")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Boy (cm)")]
            [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır.")]
            public int? Height { get; set; }

            [Required]
            [Display(Name = "Kilo (kg)")]
            [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır.")]
            public double? Weight { get; set; }

            [Required]
            [Display(Name = "Doğum Tarihi")]
            [DataType(DataType.Date)]
            public DateTime? BirthDate { get; set; }

            [Required]
            [Display(Name = "Cinsiyet")]
            public string Gender { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "Hedef açıklaması en fazla 100 karakter olabilir.")]
            [Display(Name = "Fitness Hedefi")]
            public string FitnessGoal { get; set; }
        }

        private async Task LoadAsync(AppUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            Username = userName;

            // Randevu sayısını çekiyoruz
            ActiveAppointmentCount = await _context.Appointments
                .Where(a => a.MemberId == user.Id)
                .CountAsync();

            Input = new InputModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Height = user.Height,
                Weight = user.Weight,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                FitnessGoal = user.FitnessGoal
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // --- YAŞ KONTROLÜ (12 - 100) ---
            var today = DateTime.Today;
            var age = today.Year - Input.BirthDate.Value.Year;
            if (Input.BirthDate.Value.Date > today.AddYears(-age)) age--;

            if (age < 12)
            {
                ModelState.AddModelError(string.Empty, "Hata: En az 12 yaşında olmalısınız.");
                await LoadAsync(user);
                return Page();
            }
            else if (age > 100)
            {
                ModelState.AddModelError(string.Empty, "Hata: Lütfen geçerli bir doğum yılı giriniz.");
                await LoadAsync(user);
                return Page();
            }
            // -------------------------------

            if (user.FirstName != Input.FirstName ||
                user.LastName != Input.LastName ||
                user.Height != Input.Height ||
                user.Weight != Input.Weight ||
                user.FitnessGoal != Input.FitnessGoal ||
                user.Gender != Input.Gender ||
                user.BirthDate != Input.BirthDate)
            {
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.Height = Input.Height;
                user.Weight = Input.Weight;
                user.BirthDate = Input.BirthDate;
                user.Gender = Input.Gender;
                user.FitnessGoal = Input.FitnessGoal;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Hata: Profil güncellenemedi.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Profiliniz başarıyla güncellendi";
            return RedirectToPage();
        }

        // --- HESAP SİLME İŞLEMİ ---
        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound($"Unable to load user.");

            // 1. Şifre Kontrolü
            if (!await _userManager.CheckPasswordAsync(user, DeletePassword))
            {
                ModelState.AddModelError(string.Empty, "Hata: Girdiğiniz şifre yanlış. Hesap silinemedi.");
                await LoadAsync(user);
                return Page();
            }

            // 2. Randevuları Sil (Cleanup)
            var appointments = _context.Appointments.Where(a => a.MemberId == user.Id);
            _context.Appointments.RemoveRange(appointments);
            await _context.SaveChangesAsync();

            // 3. Kullanıcıyı Sil
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred deleting user.");
            }

            // 4. Çıkış Yap ve Ana Sayfaya Gönder
            await _signInManager.SignOutAsync();
            return Redirect("~/");
        }
    }
}