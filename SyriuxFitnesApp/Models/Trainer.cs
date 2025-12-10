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
        [Display(Name = "Mesai Başlangıç")]
        [Required(ErrorMessage = "Başlangıç saati giriniz.")]
        [DataType(DataType.Time)] // Saat seçici çıkarır
        public TimeSpan WorkStartHour { get; set; } = new TimeSpan(9, 0, 0);

        [Display(Name = "Mesai Bitiş")]
        [Required(ErrorMessage = "Bitiş saati giriniz.")]
        [DataType(DataType.Time)]
        public TimeSpan WorkEndHour { get; set; } = new TimeSpan(17, 0, 0);




        // İlişkiler
        // Bir antrenörün birden fazla hizmeti olabilir
        public ICollection<TrainerService>? TrainerServices { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
    }
}
