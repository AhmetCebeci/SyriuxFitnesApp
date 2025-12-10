namespace SyriuxFitnesApp.Models
{
    // Bu sınıf ara tablodur. Hoca ID'si ile Servis ID'sini eşleştirir.
    public class TrainerService
    {
        public int TrainerId { get; set; }
        public Trainer? Trainer { get; set; }

        public int ServiceId { get; set; }
        public Service? Service { get; set; }
    }
}