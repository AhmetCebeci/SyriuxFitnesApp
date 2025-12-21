# 🏋️‍♂️ Syriux Fitness Center - AI Destekli Spor Salonu Yönetim Sistemi

![.NET Core](https://img.shields.io/badge/.NET%20Core-8.0-purple) ![EF Core](https://img.shields.io/badge/Entity%20Framework-Core-blue) ![Bootstrap](https://img.shields.io/badge/Bootstrap-5-orange) ![Gemini AI](https://img.shields.io/badge/AI-Google%20Gemini-4285F4) ![License](https://img.shields.io/badge/License-MIT-green)

> **Ders:** Web Programlama | **Dönem:** 2025-2026 Güz  
> **Öğrenci:** Ahmet Cebeci | **Numara:** B231210077 | **Grup:** C1

## 📖 Proje Hakkında

Bu proje, Sakarya Üniversitesi Bilgisayar Mühendisliği Web Programlama dersi kapsamında geliştirilmiş, **ASP.NET Core MVC 8.0** mimarisine sahip kapsamlı bir Spor Salonu Yönetim Sistemidir.

Sistem sadece bir randevu yazılımı olmanın ötesinde; **Google Gemini AI** ve **Pollinations.ai** servislerini kullanarak kullanıcılara fiziksel özelliklerine göre **kişiselleştirilmiş antrenman programı** ve **hedef vücut simülasyonu** sunan akıllı bir platformdur. Admin ve Üye panelleri ile tam kapsamlı bir yönetim deneyimi sağlar.

---

## 🚀 Öne Çıkan Özellikler

### 🤖 Yapay Zeka Destekli "Smart Trainer"
Kullanıcılar fotoğraflarını yükleyip hedeflerini (Kilo Verme, Hacim Kazanma vb.) seçtiklerinde:
1.  **Görüntü İşleme:** Google Gemini Vision API, kullanıcının vücut tipini ve yüz hatlarını analiz eder.
2.  **Program Hazırlama:** Kişiye özel beslenme ve antrenman programı oluşturulur.
3.  **Görsel Simülasyon:** Pollinations API ile kullanıcının program sonunda ulaşacağı tahmini fiziksel görünüm (yüz hatları korunarak) simüle edilir.

### 📅 Gelişmiş Randevu ve Çakışma Kontrolü
* **Conflict Detection:** Aynı antrenöre veya aynı üyeye, aynı saat diliminde mükerrer randevu alınması engellenir.
* **Snapshot Pricing:** Randevu alındığı andaki hizmet fiyatı ve süresi veritabanına "mühürlenir". Hizmete zam gelse bile eski randevular etkilenmez (Veri Bütünlüğü).
* **Dinamik Müsaitlik:** Antrenörlerin mesai saatleri ve dolu olduğu zamanlar AJAX ile anlık kontrol edilir.

### 🛠️ Yönetim ve Altyapı
* **Identity Entegrasyonu:** Güvenli kayıt, giriş ve rol bazlı (Admin/User) yetkilendirme.
* **CRUD Operasyonları:** Hizmetler, Antrenörler ve Salon bilgileri için tam yönetim.
* **REST API:** Proje içinde antrenör verilerini ve yapay zeka sonuçlarını yöneten iç API servisleri.
* **Validasyon:** Hem Client-side (jQuery) hem Server-side (Fluent/Data Annotations) veri doğrulama.

---

## 🧰 Kullanılan Teknolojiler

| Kategori | Teknoloji |
|----------|-----------|
| **Backend** | C#, ASP.NET Core 8.0, Entity Framework Core (Code-First) |
| **Frontend** | HTML5, CSS3, Bootstrap 5, JavaScript, jQuery |
| **Veritabanı** | Microsoft SQL Server |
| **Yapay Zeka** | Google Gemini 1.5 Flash API, Pollinations.ai API |
| **Güvenlik** | ASP.NET Core Identity (Role Based Authorization) |

---

## ⚙️ Kurulum ve Çalıştırma

Projeyi yerel makinenizde çalıştırmak için aşağıdaki adımları izleyin:

1.  **Repoyu Klonlayın:**
    ```bash
    git clone [https://github.com/AhmetCebeci/SyriuxFitnessApp.git](https://github.com/AhmetCebeci/SyriuxFitnessApp.git)
    ```

2.  **Veritabanını Yapılandırın:**
    `appsettings.json` dosyasındaki `DefaultConnection` kısmını kendi SQL Server bilginize göre düzenleyin.
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=.;Database=SyriuxFitnessDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
    }
    ```

3.  **Google API Key Ekleyin:**
    `appsettings.json` dosyasına Gemini API anahtarınızı ekleyin:
    ```json
    "Google": {
      "ApiKey": "BURAYA_API_KEY_GELECEK"
    }
    ```

4.  **Migration Uygulayın:**
    Package Manager Console üzerinden veritabanını oluşturun:
    ```powershell
    Update-Database
    ```

5.  **Projeyi Başlatın:**
    Projeyi çalıştırın (`F5` veya `Ctrl+F5`).

---

## 🔐 Giriş Bilgileri

Proje ayağa kalktığında veritabanına otomatik olarak Admin kullanıcısı eklenir (Seed Data):

* **Admin Hesabı:**
    * **Email:** `ogrencinumarasi@sakarya.edu.tr`
    * **Şifre:** `sau`
* **Normal Üye:**
    * Kayıt Ol sayfasından yeni üyelik oluşturabilirsiniz.

---

## 🏗 Veritabanı Şeması (Özet)

* **AppUsers:** Kullanıcı ve Admin bilgileri, fiziksel özellikler.
* **Appointments:** Randevu kayıtları (Snapshot verileri ile).
* **Trainers & Services:** Eğitmen ve Hizmet bilgileri.
* **TrainerServices:** Çoka-çok ilişki tablosu (Hangi hoca hangi dersi veriyor).

Ahmet Cebeci - [GitHub Profilim](https://github.com/AhmetCebeci)
