using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SyriuxFitnesApp.Models
{
    public class AppUser :IdentityUser
    {
        // --- Kişisel Bilgiler ---
        [Display(Name = "Ad")]
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ad en az 2, en fazla 50 karakter olabilir.")]
        public string FirstName { get; set; }

        [Display(Name = "Soyad")]
        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyad en az 2, en fazla 50 karakter olabilir.")]
        public string LastName { get; set; }

        // --- Fiziksel Bilgiler (AI ve Sağlık analizi için sınırlar önemli) ---

        [Display(Name = "Boy (cm)")]
        [Required(ErrorMessage = "Boy bilgisi zorunludur.")]
        [Range(100, 250, ErrorMessage = "Boy 100cm ile 250cm arasında olmalıdır.")]
        public int? Height { get; set; }        // Boy (cm)

        [Display(Name = "Kilo (kg)")]
        [Required(ErrorMessage = "Kilo bilgisi zorunludur.")] 
        [Range(30, 300, ErrorMessage = "Kilo 30kg ile 300kg arasında olmalıdır.")]
        public double? Weight { get; set; }     // Kilo (kg)

        [Display(Name = "Cinsiyet")]
        [Required(ErrorMessage = "Lütfen cinsiyet seçimi yapınız.")]
        public string? Gender { get; set; }     // Cinsiyet

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)] // HTML'de tarih seçici çıkmasını sağlar
        [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
        public DateTime? BirthDate { get; set; }

        // --- Hedef ---
        [Display(Name = "Fitness Hedefi")]
        [Required(ErrorMessage = "Bir hedef belirlemelisiniz (Örn: Kilo Verme,Hacim Kazanma vs).")]
        [StringLength(100, ErrorMessage = "Hedef açıklaması çok uzun.")]
        public string? FitnessGoal { get; set; } // Örn: Kilo Verme, Hacim Kazanma vs
    }
}
