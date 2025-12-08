using System.ComponentModel.DataAnnotations;

namespace SyriuxFitnesApp.Models
{
    public class Trainer
    {
        public int TrainerId { get; set; }

        [Display(Name = "Ad Soyad")]
        [Required(ErrorMessage = "Antrenör adı boş bırakılamaz!")] // Sunucu kontrolü
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        public string FullName { get; set; }

        [Display(Name = "Uzmanlık Alanı")]
        [Required(ErrorMessage = "Uzmanlık alanı seçilmelidir!")] // Sunucu kontrolü
        public string Expertise { get; set; }

        // Çalışma saatleri için alanlar
        [Display(Name = "Mesai Başlangıç Saati")]
        [Range(0, 23)]
        public int WorkStartHour { get; set; } = 9; // Varsayılan 09:00

        [Display(Name = "Mesai Bitiş Saati")]
        [Range(0, 23)]
        public int WorkEndHour { get; set; } = 18; // Varsayılan 17:00




        // İlişkiler
        // Bir antrenörün birden fazla hizmeti olabilir
        public ICollection<TrainerService>? TrainerServices { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
