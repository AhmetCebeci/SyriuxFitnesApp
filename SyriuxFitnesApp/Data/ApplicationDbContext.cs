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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Fiyat alanı için: Virgülden önce 18 basamak, virgülden sonra 2 basamak (Kuruş) ayır.
            builder.Entity<Service>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");
        }
    }
}