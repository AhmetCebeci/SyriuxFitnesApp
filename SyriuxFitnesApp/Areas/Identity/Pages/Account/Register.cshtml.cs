// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SyriuxFitnesApp.Models;

namespace SyriuxFitnesApp.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserStore<AppUser> _userStore;
        private readonly IUserEmailStore<AppUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<AppUser> userManager,
            IUserStore<AppUser> userStore,
            SignInManager<AppUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        /// 

















        public class InputModel
        {
            // --- 1. Katman Doğrulama (Attribute Validation) ---

            [Required(ErrorMessage = "Ad alanı zorunludur.")]
            [Display(Name = "Ad")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Soyad alanı zorunludur.")]
            [Display(Name = "Soyad")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Boy bilgisi zorunludur.")]
            [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır.")]
            [Display(Name = "Boy (cm)")]
            public int? Height { get; set; } // Nullable bıraktık ki boş girilirse 0 değil null gelsin, Required yakalasın.

            [Required(ErrorMessage = "Kilo bilgisi zorunludur.")]
            [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır.")]
            [Display(Name = "Kilo (kg)")]
            public double? Weight { get; set; }

            [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
            [DataType(DataType.Date)]
            [Display(Name = "Doğum Tarihi")]
            public DateTime? BirthDate { get; set; }

            [Required(ErrorMessage = "Cinsiyet seçimi zorunludur.")]
            [Display(Name = "Cinsiyet")]
            public string Gender { get; set; }

            [Required(ErrorMessage = "Bir hedef seçmelisiniz.")]
            [Display(Name = "Fitness Hedefi")]
            public string FitnessGoal { get; set; }

            // --- Standart Identity Alanları ---
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "Şifre en az {2} karakter olmalı.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Şifre")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Şifre Tekrar")]
            [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
            public string ConfirmPassword { get; set; }
        }























        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }














        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // 1. KATMAN: Attribute Validation (Model State kontrolü)
            if (ModelState.IsValid)
            {
                // --- 2. KATMAN: MANUEL DOĞRULAMA (Logic Checks) ---                

                // Hata 1: Gelecek tarih kontrolü
                if (Input.BirthDate > DateTime.Now)
                {
                    ModelState.AddModelError(string.Empty, "Hata: Doğum tarihi bugünden ileri bir tarih olamaz.");
                    return Page();
                }

                // Hata 2: Yaş Sınırı (Örn: 12 yaşından küçükler kayıt olamaz)
                var today = DateTime.Today;
                var age = today.Year - Input.BirthDate.Value.Year;
                if (Input.BirthDate.Value.Date > today.AddYears(-age)) age--;

                if (age < 12)
                {
                    ModelState.AddModelError(string.Empty, "Hata: Kayıt olmak için en az 12 yaşında olmalısınız.");
                    return Page();
                }
                // -----------------------------------------------------

                // Kullanıcı nesnesini oluştur
                var user = CreateUser();

                // --- VERİ EŞLEME (MAPPING) ---
                // Formdan (Input) gelen verileri Veritabanı nesnesine (AppUser) aktarıyoruz
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.Height = Input.Height.Value; // InputModel'de Required olduğu için Value güvenli
                user.Weight = Input.Weight.Value;
                user.BirthDate = Input.BirthDate.Value;
                user.Gender = Input.Gender;
                user.FitnessGoal = Input.FitnessGoal;
                // -----------------------------

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // Şifreyi hashleyip kullanıcıyı oluşturur
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // --- ROL ATAMA İŞLEMİ ---
                    // Yeni gelen herkes standart 'Member' olsun.
                    await _userManager.AddToRoleAsync(user, "Member");
                    // ------------------------

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }

                // Eğer Identity hatası varsa (Şifre yetersiz, email kullanımda vs.)
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Bir şeyler ters gittiyse formu tekrar göster
            return Page();
        }















        private AppUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<AppUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AppUser)}'. " +
                    $"Ensure that '{nameof(AppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<AppUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AppUser>)_userStore;
        }
    }
}
