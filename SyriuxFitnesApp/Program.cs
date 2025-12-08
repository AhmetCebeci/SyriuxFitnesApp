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

// (2. ADIM: Çerez Ayarları) ---
// Admin için "Beni Hatırla" kapalı olduğu için buradaki süre sadece oturum açıkken geçerli olur.
// Memberlar için ise standart süre işler.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});
// -----------------------------

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // <-- Giriş yapma mekanizması (Kimlik Doğrulama)
app.UseAuthorization(); // <-- Yetkilendirme

// --- (Areas Rotası) ---
// Admin paneline giden istekleri yakalamak için bu kuralı EN ÜSTE koyuyoruz.
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
// ----------------------

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // <-- Identity sayfalarının çalışması için bu gereklidir

// --- SEEDING İŞLEMİ (Admin ve Rolleri Yükle) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Seed hatası: " + ex.Message);
    }
}
// ------------------------------------------------

app.Run();