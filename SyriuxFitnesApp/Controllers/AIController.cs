using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SyriuxFitnesApp.Models;
// Google GenAI Resmi Kütüphanesi
using Google.GenAI;
using Google.GenAI.Types;

namespace SyriuxFitnesApp.Controllers
{
    [Authorize]
    [Route("AI")]
    public class AIController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        // SENİN API KEYİN (Kodun içinde tanımlı)
        private const string ApiKey = "AIzaSyBQ6gAZqGsDyrhq4etWdZ5Tkbk2ip84_zc";

        public AIController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        [HttpPost]
        [Route("GetAdvice")]
        public async Task<IActionResult> GetAdvice(string adviceType)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Kullanıcı bulunamadı." });

            string prompt = PreparePrompt(user, adviceType);

            // Yeni SDK ile Gemini 2.5 çağıran fonksiyon
            string result = await CallGoogleGeminiSDK(prompt, adviceType);

            return Json(new { success = true, content = result });
        }

        private string PreparePrompt(AppUser user, string type)
        {
            string gender = user.Gender ?? "Belirtilmemiş";
            string goal = user.FitnessGoal ?? "Sağlıklı Yaşam";

            if (type == "workout")
            {
                return $"Ben bir spor antrenörüyüm. Danışanım: {gender}, Boy: {user.Height}cm, Kilo: {user.Weight}kg, Hedef: {goal}. Bu kişiye uygun haftalık antrenman programı hazırla. Cevabı SADECE HTML formatında (ul, li, strong, h5 etiketleri ile) ver. Markdown kullanma. Asla ```html yazma. Başlıkları h5 ile yaz.";
            }
            else
            {
                return $"Ben bir diyetisyenim. Danışanım: {gender}, Boy: {user.Height}cm, Kilo: {user.Weight}kg, Hedef: {goal}. Günlük örnek beslenme programı (Kahvaltı, Öğle, Akşam, Ara öğün) hazırla. Cevabı SADECE HTML formatında (ul, li, strong, h5 etiketleri ile) ver. Markdown kullanma. Asla ```html yazma.";
            }
        }

        // --- RESMİ GOOGLE.GENAI SDK KULLANAN METOT ---
        private async Task<string> CallGoogleGeminiSDK(string prompt, string type)
        {
            if (string.IsNullOrEmpty(ApiKey))
            {
                await Task.Delay(1000);
                return GetDemoData(type) + "<br><div class='alert alert-info'>Demo Modu (Key Girilmedi)</div>";
            }

            try
            {
                // 1. İSTEMCİYİ OLUŞTUR
                var client = new Google.GenAI.Client(apiKey: ApiKey);

                // 2. İSTEĞİ GÖNDER (Model: gemini-2.5-flash)
                // C# SDK'sında contents parametresi string kabul edebilir.
                var response = await client.Models.GenerateContentAsync(
                    model: "gemini-2.5-flash",
                    contents: prompt
                );

                // 3. CEVABI AL (HATA ÇÖZÜMÜ BURADA)
                // Hata veren 'response.Text' yerine SDK'nın gerçek yolunu kullanıyoruz:
                // Candidates -> [0] -> Content -> Parts -> [0] -> Text
                string textResponse = "";

                if (response?.Candidates != null && response.Candidates.Count > 0)
                {
                    var firstCandidate = response.Candidates[0];
                    if (firstCandidate.Content?.Parts != null && firstCandidate.Content.Parts.Count > 0)
                    {
                        textResponse = firstCandidate.Content.Parts[0].Text;
                    }
                }

                if (string.IsNullOrEmpty(textResponse))
                {
                    throw new Exception("Yapay zeka boş cevap döndü.");
                }

                // Temizlik
                return textResponse.Replace("```html", "").Replace("```", "").Trim();
            }
            catch (Exception ex)
            {
                // Hata olursa (Örn: Model bulunamazsa veya kota biterse) DEMO veriyi göster.
                string errorHtml = $"<div class='alert alert-warning mt-3 border-danger'><i class='bi bi-exclamation-circle'></i> <strong>Yapay Zeka Hatası:</strong><br>{ex.Message}<br><em>(Sistem demo modunda yanıt veriyor)</em></div>";
                return GetDemoData(type) + errorHtml;
            }
        }

        // --- DEMO VERİLERİ (Güvenlik Ağı) ---
        private string GetDemoData(string type)
        {
            string html = "";
            if (type == "workout")
            {
                html = @"
                <div class='alert alert-success border-0 shadow-sm'>
                    <i class='bi bi-activity'></i> <strong>Kişisel Antrenman Programınız Hazır!</strong>
                </div>
                <h5 class='text-primary mt-3'>Haftalık Plan Önerisi</h5>
                <ul class='list-group list-group-flush'>
                    <li class='list-group-item'><strong class='text-dark'>Pazartesi:</strong> Tüm Vücut (Squat, Push-up, Row) + 20dk Kardiyo.</li>
                    <li class='list-group-item'><strong class='text-dark'>Salı:</strong> Dinlenme veya Hafif Yürüyüş (45 dk).</li>
                    <li class='list-group-item'><strong class='text-dark'>Çarşamba:</strong> Alt Vücut Odaklı (Lunge, Deadlift) + Karın Egzersizleri.</li>
                    <li class='list-group-item'><strong class='text-dark'>Perşembe:</strong> Aktif Dinlenme (Yoga/Esneme).</li>
                    <li class='list-group-item'><strong class='text-dark'>Cuma:</strong> Üst Vücut (Omuz Press, Biceps Curl, Triceps).</li>
                    <li class='list-group-item'><strong class='text-dark'>Hafta Sonu:</strong> Doğa yürüyüşü veya Yüzme.</li>
                </ul>";
            }
            else
            {
                html = @"
                <div class='alert alert-success border-0 shadow-sm'>
                    <i class='bi bi-apple'></i> <strong>Kişisel Beslenme Programınız Hazır!</strong>
                </div>
                <h5 class='text-primary mt-3'>Günlük Beslenme Planı</h5>
                <ul class='list-group list-group-flush'>
                    <li class='list-group-item'><strong class='text-dark'>Kahvaltı:</strong> 2 Yumurta (Haşlanmış), 1 dilim tam buğday ekmeği, bol yeşillik, 5 zeytin.</li>
                    <li class='list-group-item'><strong class='text-dark'>Öğle:</strong> Izgara Tavuk/Köfte (150gr), Salata (Az yağlı), 4 kaşık bulgur pilavı.</li>
                    <li class='list-group-item'><strong class='text-dark'>İkindi:</strong> 1 kase yoğurt veya 1 bardak kefir.</li>
                    <li class='list-group-item'><strong class='text-dark'>Akşam:</strong> Zeytinyağlı sebze yemeği (Kabak/Ispanak), Yoğurt. (Ekmek yok).</li>
                </ul>";
            }
            return html;
        }
    }
}