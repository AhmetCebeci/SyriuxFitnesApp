using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SyriuxFitnesApp.Models;
using Google.GenAI;
using System.Text.Json;

namespace SyriuxFitnesApp.Controllers
{
    [Authorize]
    [Route("AI")]
    public class AIController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

        public AIController(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        [HttpPost]
        [Route("GetAdvice")]
        public async Task<IActionResult> GetAdvice(string adviceType, IFormFile userImage)
        {
            // 1. Kullanıcı Kontrolü
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Kullanıcı bulunamadı." });

            // 2. Fotoğraf Kontrolü
            if (userImage == null || userImage.Length == 0)
            {
                return Json(new { success = false, message = "Lütfen fotoğrafınızı yükleyin." });
            }

            // 3. Gemini'den Program Al
            string prompt = PreparePrompt(user, adviceType);
            var aiResult = await CallGoogleGeminiSDK(prompt, adviceType);

            // 4. ÜCRETSİZ Resim Üret (Pollinations.ai)
            string finalImage = null;
            string debugError = aiResult.ErrorMessage;

            if (!aiResult.IsDemo && !string.IsNullOrEmpty(aiResult.ImagePrompt))
            {
                try
                {
                    finalImage = await GenerateImageWithPollinations(aiResult.ImagePrompt);
                }
                catch (Exception ex)
                {
                    debugError = (debugError ?? "") + " | Resim Hatası: " + ex.Message;
                }
            }

            return Json(new
            {
                success = true,
                content = aiResult.Content,
                generatedImage = finalImage,
                debugError = debugError
            });
        }

        private string PreparePrompt(AppUser user, string type)
        {
            string gender = user.Gender ?? "Sporcu";
            string goal = user.FitnessGoal ?? "Form Tutma";
            string duration = type == "workout" ? "6 ay" : "2 ay";
            string context = type == "workout" ? "Antrenman" : "Diyet";

            return $@"
            Sen uzman bir antrenörsün. Danışan: {gender}, {user.Height}cm, {user.Weight}kg, Hedef: {goal}.
            
            GÖREV 1: Bu kişiye uygun {context} programı hazırla.
            Cevabı SADECE HTML formatında (ul, li, strong, h5 vb.) ver.

            GÖREV 2: Bu kişinin programı uyguladıktan sonra ({duration} sonra) nasıl görüneceğini hayal et.
            Cevabın en altına '|||' işareti koy ve devamına bu hayali resmetmek için net bir İNGİLİZCE IMAGE PROMPT yaz.
            
            ÖRNEK FORMAT:
            <h5>Program</h5>
            <ul><li>...</li></ul>
            |||
            Full body photo of a fit {gender}, athletic physique, confident pose, gym background, professional lighting
            ";
        }

        // ÜCRETSİZ RESİM ÜRETİMİ - Pollinations.ai
        private async Task<string?> GenerateImageWithPollinations(string prompt)
        {
            try
            {
                string encodedPrompt = Uri.EscapeDataString(prompt);
                string imageUrl = $"https://image.pollinations.ai/prompt/{encodedPrompt}?width=512&height=512&nologo=true";

                var response = await _httpClient.GetAsync(imageUrl);

                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        return Convert.ToBase64String(imageBytes);
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private class GeminiResult
        {
            public string Content { get; set; }
            public string ImagePrompt { get; set; }
            public bool IsDemo { get; set; }
            public string ErrorMessage { get; set; }
        }

        private async Task<GeminiResult> CallGoogleGeminiSDK(string prompt, string type)
        {
            string apiKey = _configuration["Google:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return new GeminiResult { Content = GetDemoData(type), IsDemo = true, ErrorMessage = "Google API Key eksik." };
            }

            try
            {
                var client = new Google.GenAI.Client(apiKey: apiKey);
                var response = await client.Models.GenerateContentAsync("gemini-2.5-flash", prompt);

                string textResponse = "";
                if (response?.Candidates != null && response.Candidates.Count > 0)
                {
                    textResponse = response.Candidates[0].Content?.Parts?[0]?.Text ?? "";
                }

                if (string.IsNullOrEmpty(textResponse)) throw new Exception("Yapay zeka boş cevap döndü.");

                string cleanText = textResponse.Replace("```html", "").Replace("```", "").Trim();

                if (cleanText.Contains("|||"))
                {
                    var parts = cleanText.Split("|||", StringSplitOptions.RemoveEmptyEntries);
                    return new GeminiResult { Content = parts[0].Trim(), ImagePrompt = parts.Length > 1 ? parts[1].Trim() : "", IsDemo = false };
                }

                return new GeminiResult { Content = cleanText, ImagePrompt = "", IsDemo = false };
            }
            catch (Exception ex)
            {
                return new GeminiResult
                {
                    Content = GetDemoData(type),
                    IsDemo = true,
                    ErrorMessage = "Gemini Hatası: " + ex.Message
                };
            }
        }

        private string GetDemoData(string type)
        {
            if (type == "workout")
            {
                return @"<div class='alert alert-warning'>Demo Modu (Hata oluştu)</div>
                         <h5>Örnek Antrenman Programı</h5>
                         <ul>
                             <li><strong>Pazartesi:</strong> Göğüs - Bench Press 3x10, Push-ups 3x15</li>
                             <li><strong>Çarşamba:</strong> Sırt - Pull-ups 3x8, Rows 3x12</li>
                             <li><strong>Cuma:</strong> Bacak - Squats 3x12, Lunges 3x10</li>
                         </ul>";
            }
            else
            {
                return @"<div class='alert alert-warning'>Demo Modu (Hata oluştu)</div>
                         <h5>Örnek Diyet Programı</h5>
                         <ul>
                             <li><strong>Sabah:</strong> Yulaf ezmesi + süt + meyve</li>
                             <li><strong>Öğle:</strong> Tavuk göğüs + salata + pilav</li>
                             <li><strong>Akşam:</strong> Balık + sebze + yoğurt</li>
                         </ul>";
            }
        }
    }
}

