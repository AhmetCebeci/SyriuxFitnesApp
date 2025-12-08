using System.ComponentModel.DataAnnotations;

namespace SyriuxFitnesApp.Models
{
    public class Salon
    {
        public int Id { get; set; }

        [Display(Name = "Salon Adı")]
        [Required]
        public string SalonName { get; set; }

        [Display(Name = "Açılış Saati")]
        [Required]
        [DataType(DataType.Time)]
        public TimeSpan OpeningTime { get; set; }

        [Display(Name = "Kapanış Saati")]
        [Required]
        [DataType(DataType.Time)]
        public TimeSpan ClosingTime { get; set; }
    }
}