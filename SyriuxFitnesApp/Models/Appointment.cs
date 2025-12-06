using System.ComponentModel.DataAnnotations;

namespace SyriuxFitnesApp.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; } // Örn: "14:00"

        public bool IsApproved { get; set; } = false; // Onay durumu [cite: 21]

        // İlişkiler (Hangi üye? Hangi hoca? Hangi ders?)
        public string? MemberId { get; set; }
        public AppUser? Member { get; set; }

        public int TrainerId { get; set; }
        public Trainer? Trainer { get; set; }

        public int ServiceId { get; set; }
        public Service? Service { get; set; }
    }
}

