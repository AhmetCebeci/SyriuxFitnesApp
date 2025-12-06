using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection; // GetRequiredService için gerekli
using SyriuxFitnesApp.Models;

namespace SyriuxFitnesApp.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            // Servisleri çağırıyoruz
            var userManager = service.GetRequiredService<UserManager<AppUser>>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Roller yoksa oluşturuyoruz
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Member"))
                await roleManager.CreateAsync(new IdentityRole("Member"));

            // 2. Admin Kullanıcısını oluşturuyoruz
            var adminEmail = "b231210077@sakarya.edu.tr";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdmin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Sistem",
                    LastName = "Yöneticisi",

                    // --- ZORUNLU ALANLARI DOLDURMAMIZ LAZIM ---
                    // AppUser modelinde bunları required yaptığımız için 
                    // burası boş kalırsa veritabanı hatası alırız.
                    Height = 180,
                    Weight = 80,
                    BirthDate = new DateTime(2000, 1, 1), // Örnek tarih
                    Gender = "Erkek",
                    FitnessGoal = "Sistem Yönetimi"
                    // ------------------------------------------
                };

                // Şifre: sau 
                var result = await userManager.CreateAsync(newAdmin, "sau");

                // Admin rolünü veriyoruz
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }
        }
    }
}