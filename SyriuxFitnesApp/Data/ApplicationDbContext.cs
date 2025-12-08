using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SyriuxFitnesApp.Models;

namespace SyriuxFitnesApp.Data
{
    // IdentityDbContext sınıfından miras alıyoruz, böylece Üyelik tabloları otomatik gelecek
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Bizim oluşturduğumuz tabloları veritabanına tanıtıyoruz
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Salon> Salons { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Fiyat alanı için: Virgülden önce 18 basamak, virgülden sonra 2 basamak (Kuruş) ayır.
            builder.Entity<Service>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");

            // Salon için varsayılan bir ayar (Migration sırasında hata vermemesi için)
            builder.Entity<Salon>().HasData(
                new Salon { Id = 1, SalonName = "Syriux Fitness", OpeningTime = new TimeSpan(9, 0, 0), ClosingTime = new TimeSpan(22, 0, 0) }
            );

            // --- MANY-TO-MANY AYARI ---
            builder.Entity<TrainerService>()
                .HasKey(ts => new { ts.TrainerId, ts.ServiceId }); // İkisi birlikte Primary Key olsun

            builder.Entity<TrainerService>()
                .HasOne(ts => ts.Trainer)
                .WithMany(t => t.TrainerServices)
                .HasForeignKey(ts => ts.TrainerId);

            builder.Entity<TrainerService>()
                .HasOne(ts => ts.Service)
                .WithMany(s => s.TrainerServices)
                .HasForeignKey(ts => ts.ServiceId);
            // --------------------------
        }
    }
}