using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        // Bu satır, sayıyı kutuya yazarken "500,00" yerine "500" (veya kuruş varsa "500.50") olarak yazar.
        [DisplayFormat(DataFormatString = "{0:0.####}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }

        // İlişkiler
        // Bir hizmetin birden fazla randevusu olabilir.
        public ICollection<Appointment>? Appointments { get; set; }

        // Bir hizmeti birden fazla antrenör verebilir
        public ICollection<TrainerService>? TrainerServices { get; set; }
    }
}