<div align="center">

# 🏋️‍♂️ Syriux Fitness Center
### AI Destekli Yeni Nesil Spor Salonu Yönetim Sistemi

![.NET Core](https://img.shields.io/badge/.NET%20Core-8.0-512BD4?style=for-the-badge&logo=.net&logoColor=white)
![Google Gemini](https://img.shields.io/badge/Google%20Gemini-AI-4285F4?style=for-the-badge&logo=google&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)

<p align="center">
  <a href="#-proje-hakkında">Proje Hakkında</a> •
  <a href="#-teknik-mimari-ve-altyapı">Teknik Mimari</a> •
  <a href="#-özellikler">Özellikler</a> •
  <a href="#-kurulum">Kurulum</a> •
  <a href="#-veritabanı-yapısı">Veritabanı</a>
</p>

</div>

---

> **🎓 Akademik Künye**
>
> | Alan | Detay |
> |---|---|
> | **Üniversite** | Sakarya Üniversitesi - Bilgisayar Mühendisliği |
> | **Ders** | Web Programlama (BSM) |
> | **Dönem** | 2025-2026 Güz |
> | **Geliştirici** | Ahmet Cebeci (B231210077) |
> | **Grup** | C1 |

---

## 📖 Proje Hakkında

**Syriux Fitness App**, geleneksel spor salonu yönetim sistemlerinin ötesine geçerek, **Yapay Zeka (AI)** teknolojilerini iş süreçlerine entegre eden kapsamlı bir web uygulamasıdır. **ASP.NET Core MVC 8.0** mimarisi üzerine inşa edilen bu proje, hem spor salonu yöneticileri için güçlü bir idari panel hem de üyeler için kişiselleştirilmiş bir dijital antrenör deneyimi sunar.

Projenin temel amacı, sadece randevu takibi yapmak değil; kullanıcının fiziksel verilerini ve hedeflerini analiz ederek **Google Gemini LLM** ve **Vision API** aracılığıyla "Kişiye Özel" reçeteler sunmak ve sonuçları simüle etmektir.

---

## 🏗 Teknik Mimari ve Altyapı

Proje, **Monolitik** bir yapıda kurgulanmış olup, **Layered Architecture (Katmanlı Mimari)** prensiplerine sadık kalınarak geliştirilmiştir. Gelecekteki mobil entegrasyonlar için API endpoint'leri içermektedir.

### 🔧 Backend (Arka Uç)
* **Framework:** ASP.NET Core 8.0 MVC
* **ORM:** Entity Framework Core (Code-First Yaklaşımı)
* **Veritabanı:** Microsoft SQL Server
* **Güvenlik:** ASP.NET Core Identity (Role-Based Authorization - Admin/Member)
* **Design Patterns:** Dependency Injection (DI), Repository Pattern mantığı, ViewModel kullanımı.

### 🎨 Frontend (Ön Yüz)
* **UI Framework:** Bootstrap 5 & Custom CSS
* **Scripting:** JavaScript (ES6+), jQuery
* **Etkileşim:** AJAX (Sayfa yenilenmeden müsaitlik kontrolü ve dinamik veri çekme)

### 🤖 Yapay Zeka Servisleri (AI Integration)
1.  **Google Gemini 1.5 Flash:** Kullanıcının metin tabanlı verilerini (Boy, Kilo, Hedef) analiz ederek HTML formatında yapılandırılmış diyet ve antrenman programı oluşturur.
2.  **Google Gemini Vision:** Kullanıcı fotoğraflarını analiz ederek vücut tipi tespiti yapar.
3.  **Pollinations.ai:** Kullanıcının hedeflediği vücut tipine ulaştığında nasıl görüneceğini simüle eden görsel üretim servisi.

---

## 🚀 Özellikler ve İşlevsellik

### 1. Akıllı Randevu Yönetimi (Smart Scheduling)
Bu modül, basit bir takvim uygulamasının ötesinde, gerçek dünya senaryolarını simüle eden karmaşık bir iş mantığına (Business Logic) sahiptir:

* **🛡️ Conflict Detection (Çakışma Kontrolü):**
    * **Antrenör Çakışması:** Bir antrenörün aynı saat aralığında (Duration dahil) başka bir randevusu varsa sistem otomatik olarak bloklar.
    * **Üye Çakışması:** Bir üyenin aynı saatte başka bir derste olması engellenir.
    * **Salon Saatleri:** Salonun açılış/kapanış saatleri ve antrenörün mesai saatleri dinamik olarak kontrol edilir.
* **📸 Snapshot Pricing (Fiyat Mühürleme):** Randevu oluşturulduğu andaki hizmet fiyatı ve süresi veritabanına kaydedilir. İleride hizmete zam gelse bile, geçmiş veya ileri tarihli alınmış randevular bu değişiklikten etkilenmez.
* **⚡ Dinamik Slot Hesaplama:** Seçilen hizmetin süresine göre (30dk, 45dk, 60dk) müsait saat dilimleri AJAX ile backend'den anlık hesaplanarak getirilir.

### 2. AI "Smart Trainer" Modülü
Üyeler, pahalı özel dersler (PT) almak yerine yapay zekadan destek alabilir:
* **Analiz:** Kullanıcı form verileri (Yaş, Cinsiyet, Hedef, Aktivite Düzeyi) JSON formatında AI servisine gönderilir.
* **Program Üretimi:** Yapay zeka, profesyonel bir antrenör gibi davranarak `<ul>`, `<li>` etiketleri ile formatlanmış haftalık program çıktısı üretir.
* **Hata Toleransı (Fault Tolerance):** Eğer AI servisi (Google API) yanıt vermezse veya kota aşımı olursa, sistem çökmez; "Fallback" mekanizması devreye girerek kullanıcıya önceden hazırlanmış demo verileri sunar.

### 3. Yönetim Paneli (Admin Dashboard)
* **CRUD İşlemleri:** Antrenör, Hizmet (Ders) ve Salon bilgilerinin yönetimi.
* **Randevu Onay Mekanizması:** Üyelerin aldığı randevular "Onay Bekliyor" statüsüne düşer, admin onayı ile kesinleşir.
* **Raporlama:** Üye sayısı, aktif randevular ve antrenör performansları.

---

## 💻 Koddan Örnekler (Business Logic)

Randevu oluşturulurken kullanılan çakışma mantığının basitleştirilmiş hali:

```csharp
// Örnek: Antrenörün o saatte dolu olup olmadığının kontrolü
foreach (var existing in existingAppointments)
{
    // Mevcut randevunun bitiş saati
    TimeSpan existingEnd = existingStart.Add(TimeSpan.FromMinutes(duration));

    // Kesişim Formülü: (YeniBaşlangıç < EskiBitiş) VE (EskiBaşlangıç < YeniBitiş)
    if (selectedTime < existingEnd && existingStart < endTime)
    {
        throw new BusinessException("Antrenör bu saatte dolu!");
    }
}
