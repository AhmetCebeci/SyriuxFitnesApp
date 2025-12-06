using System.ComponentModel.DataAnnotations;

namespace SyriuxFitnesApp.Models
{
    public class Service
    {
        public int ServiceId { get; set; }

        [Display(Name = "Hizmet Adı")]
        [Required(ErrorMessage = "Hizmet adı zorunludur!")]
        public string ServiceName { get; set; }

        [Display(Name = "Süre (Dakika)")]
        [Required]
        [Range(15, 120, ErrorMessage = "Süre 15 ile 120 dakika arasında olmalı.")] // Mantıksal kontrol
        public int DurationMinutes { get; set; }

        [Display(Name = "Ücret (TL)")]
        [Required]
        [Range(0, 10000, ErrorMessage = "Geçerli bir ücret giriniz.")]
        public decimal Price { get; set; }
    }
}
