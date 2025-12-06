using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SyriuxFitnesApp.Data;
using SyriuxFitnesApp.Models;

var builder = WebApplication.CreateBuilder(args);




// Email hatasını çözen satır
builder.Services.AddTransient<IEmailSender, SyriuxFitnesApp.EmailServices.EmailSender>();





// 1. Veritabanı Bağlantısını Ekliyoruz
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


// 2. Kimlik Doğrulama (Identity) Ayarlarını Ekliyoruz (Roller aktif olsun diye)
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Şifre kurallarını basitleştiriyoruz 
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();









// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // <-- Giriş yapma mekanizması (Kimlik Doğrulama)
app.UseAuthorization(); // <-- Yetkilendirme

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // <-- Identity sayfalarının çalışması için bu gereklidir









// --- SEEDING ��LEM� (Admin ve Rolleri Y�kle) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Az önce yazdığımız Seed metodunu çalıştırıyoruz
        await DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        // Hata olursa konsola yazsın 
        Console.WriteLine("Seed hatası: " + ex.Message);
    }
}
// ------------------------------------------------








app.Run();
