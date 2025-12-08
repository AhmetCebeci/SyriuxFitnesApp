using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Veritabanı tür ayarları için (decimal)

namespace SyriuxFitnesApp.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        // --- TARİH VE SAAT ---
        [Display(Name = "Randevu Tarihi")]
        [Required(ErrorMessage = "Lütfen bir tarih seçiniz.")]
        [DataType(DataType.Date)] // Sadece tarih seçici çıkar (Saat çıkmaz)
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Randevu Saati")]
        [Required(ErrorMessage = "Lütfen bir saat seçiniz.")]
        public string AppointmentTime { get; set; } // Örn: "14:00"

        [Display(Name = "Onay Durumu")]
        public bool IsApproved { get; set; } = false; // Varsayılan: Onay Bekliyor

        // --- SNAPSHOT ALANLARI (Fiyat ve Süre Koruması) ---
        // Bu alanlar formda kullanıcıya gösterilmez, arka planda otomatik dolar.
        // Ama veritabanında saklanır.

        [Display(Name = "İşlem Ücreti")]
        [Column(TypeName = "decimal(18,2)")] // Para birimi formatı
        public decimal StoredPrice { get; set; }

        [Display(Name = "Hizmet Süresi (Dk)")]
        public int StoredDuration { get; set; }

        // --- İLİŞKİLER (Foreign Keys) ---

        [Display(Name = "Üye")]
        public string? MemberId { get; set; }
        [ForeignKey("MemberId")]
        public AppUser? Member { get; set; }

        [Display(Name = "Antrenör")]
        [Required(ErrorMessage = "Bir antrenör seçmelisiniz.")]
        public int TrainerId { get; set; }
        [ForeignKey("TrainerId")]
        public Trainer? Trainer { get; set; }

        [Display(Name = "Hizmet")]
        [Required(ErrorMessage = "Bir hizmet (ders) seçmelisiniz.")]
        public int ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public Service? Service { get; set; }
    }
}