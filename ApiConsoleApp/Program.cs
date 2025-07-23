using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ApiConsoleApp.models2;
using Newtonsoft.Json;

namespace ApiConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. appsettings.json dosyasını yapılandırma olarak oku
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfiguration configuration = builder.Build();

            // 2. Bağlantı dizisini al
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("Hata: appsettings.json dosyasında 'DefaultConnection' bağlantı dizisi bulunamadı. Lütfen kontrol edin.");
                Console.WriteLine("Uygulama sonlandırılıyor.");
                return;
            }

            Console.WriteLine("Uygulama başlatıldı. API'den kullanıcı verileri çekiliyor...");

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string apiUrl = "https://randomuser.me/api?results=1"; // 1 KULLANICI GELSİN
                   // Console.WriteLine($"API URL'si: {apiUrl}");
                  //  Console.WriteLine("API'ye istek gönderiliyor...");

                    string jsonString = await client.GetStringAsync(apiUrl);
                  //  Console.WriteLine("API'den yanıt alındı. JSON ayrıştırılıyor...");

                    // API'den gelen ham JSON'u görmek isterseniz, aşağıdaki yorumu kaldırın:
                    // Console.WriteLine($"Gelen Ham JSON: {jsonString}");

                    // Newtonsoft.Json kullanarak de-serileştirme
                    RandomUserApiResponse apiResponse = JsonConvert.DeserializeObject<RandomUserApiResponse>(jsonString);
                  //  Console.WriteLine("JSON başarıyla RandomUserApiResponse nesnesine dönüştürüldü.");

                    if (apiResponse == null)
                    {
                        Console.WriteLine("Hata: API yanıtı boş veya hatalı geldi. apiResponse nesnesi null.");
                    }
                    else
                    {
                      //  Console.WriteLine("apiResponse nesnesi başarıyla dolduruldu.");

                        //  INFO VERİSİNİ KAYDETME KISMI 
                        Console.WriteLine("Info nesnesi kontrol ediliyor...");
                        if (apiResponse.info != null)
                        {
                           // Console.WriteLine("Info nesnesi dolu. Info verisi veritabanına kaydedilecek.");
                            //Console.WriteLine($"Info Detayları (API'den gelen): Seed='{apiResponse.info.seed ?? "N/A"}', Results={apiResponse.info.results}, Page={apiResponse.info.page}, Version='{apiResponse.info.version ?? "N/A"}'");
                            await SaveInfoToDatabase(apiResponse.info, connectionString);
                        }
                        else
                        {
                            Console.WriteLine("Info nesnesi null veya API yanıtında 'info' özelliği bulunamadı. Info verisi kaydedilemedi.");
                        }

                        // RESULT verilerini kaydetme
                       // Console.WriteLine("Kullanıcı (results) nesnesi kontrol ediliyor...");
                        if (apiResponse.results != null && apiResponse.results.Any())
                        {
                            Console.WriteLine($"API'den {apiResponse.results.Count} adet kullanıcı verisi başarıyla çekildi.");
                            foreach (var user in apiResponse.results)
                            {
                                await SaveUserToDatabase(user, connectionString);
                            }
                            Console.WriteLine("Tüm kullanıcılar veritabanına kaydedildi.");
                        }
                        else
                        {
                            Console.WriteLine("API'den kullanıcı verisi çekilemedi veya 'results' listesi boş döndü.");
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Hata: API'ye bağlanırken bir HTTP isteği hatası oluştu: {e.Message}");
                    Console.WriteLine($"HTTP Hata Kodu: {e.StatusCode}");
                    Console.WriteLine($"Hata Detayı: {e.InnerException?.Message ?? "Yok"}");
                    Console.WriteLine("API'nizin çalıştığından ve doğru URL'yi kullandığınızdan emin olun.");
                }
                catch (JsonSerializationException e) // JSON ayrıştırma hatalarını yakalamak için
                {
                    Console.WriteLine($"Hata: JSON ayrıştırma hatası oluştu: {e.Message}");
                    Console.WriteLine($"JSON Hata Yolu: {e.Path}");
                    Console.WriteLine($"JSON Hata Satırı/Sütunu: Satır {e.LineNumber}, Sütun {e.LinePosition}");
                    Console.WriteLine($"Hata Detayı: {e.InnerException?.Message ?? "Yok"}");
                    Console.WriteLine("Model sınıflarınızın (örneğin RandomUserApiResponse, User, Info) API yanıtı JSON yapısıyla uyumlu olduğundan emin olun.");
                }
                catch (NpgsqlException e) // Genel Npgsql hatalarını yakalamak için
                {
                    Console.WriteLine($"Hata: Veritabanı (PostgreSQL) işlemi sırasında bir Npgsql hatası oluştu: {e.Message}");
                    Console.WriteLine($"SQL State: {e.SqlState}");
                    Console.WriteLine($"Detay: {e.StackTrace}");
                    Console.WriteLine("Veritabanı bağlantı dizenizi ve tablo/sütun adlarınızın doğruluğunu kontrol edin.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Hata: Beklenmeyen genel bir hata oluştu: {e.Message}");
                    Console.WriteLine($"Hata Tipi: {e.GetType().Name}");
                    Console.WriteLine($"Hata Detayı: {e.InnerException?.Message ?? "Yok"}");
                    Console.WriteLine($"Stack Trace: {e.StackTrace}");
                }
            }

            Console.WriteLine("Tüm işlemler tamamlandı. Çıkmak için bir tuşa basın...");
            Console.ReadKey();
        }

        // SaveInfoToDatabase Metodu (önceki haliyle doğruydu)
        static async Task SaveInfoToDatabase(Info info, string connectionString)
        {
            Console.WriteLine("\n--- Info Verisini Veritabanına Kaydediliyor ---");
            Console.WriteLine($"Info Seed: {info.seed ?? "N/A"}");
            Console.WriteLine($"Info Results: {info.results}");
            Console.WriteLine($"Info Page: {info.page}");
            Console.WriteLine($"Info Version: {info.version ?? "N/A"}");
            Console.WriteLine("---------------------------------------------");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    //Console.WriteLine("Veritabanı bağlantısı Info için başarıyla açıldı.");

                    using (var cmd = new NpgsqlCommand(
                        "INSERT INTO \"Info\" (\"seed\", \"results\", \"page\", \"version\") VALUES (@seed, @results, @page, @version)", conn))
                    {
                        cmd.Parameters.AddWithValue("seed", (object)info.seed ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("results", info.results);
                        cmd.Parameters.AddWithValue("page", info.page);
                        cmd.Parameters.AddWithValue("version", (object)info.version ?? DBNull.Value);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Info verisi veritabanına başarıyla kaydedildi ({rowsAffected} satır etkilendi).");
                        }
                        else
                        {
                            Console.WriteLine("Uyarı: Info verisi veritabanına kaydedilemedi (0 satır etkilendi).");
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    Console.WriteLine($"Hata: Veritabanına Info kaydederken Npgsql hatası oluştu: {ex.Message}");
                    Console.WriteLine($"SQL State: {ex.SqlState}");
                    Console.WriteLine($"Detay: {ex.StackTrace}");
                    Console.WriteLine("Lütfen 'Info' tablonuzun ve 'InfoId' sütununuzun doğru yapılandırıldığından emin olun (otomatik artan birincil anahtar).");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Hata: Veritabanına Info kaydederken beklenmeyen bir hata oluştu: {ex.Message}");
                    Console.WriteLine($"Hata Detayı: {ex.InnerException?.Message ?? "Yok"}");
                    Console.WriteLine($"Hata Tipi: {ex.GetType().Name}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                }
            }
        }

        // SaveUserToDatabase Metodu - Detaylı çıktılar geri eklendi!
        static async Task SaveUserToDatabase(User user, string connectionString)
        {
            Console.WriteLine("\n--- Yeni Kullanıcı Verisi Kaydediliyor ---");
            Console.WriteLine($"Ad Soyad: {user.Name?.title} {user.Name?.first} {user.Name?.last ?? "N/A"}");
            Console.WriteLine($"E-posta: {user.email ?? "N/A"}");
            Console.WriteLine($"Cinsiyet: {user.gender ?? "N/A"}");
            Console.WriteLine($"Yaş: {user.Dob?.age?.ToString() ?? "N/A"}");
            Console.WriteLine($"Telefon: {user.phone ?? "N/A"}");
            Console.WriteLine($"Cep Telefonu: {user.cell ?? "N/A"}");
            Console.WriteLine($"Kullanıcı Adı: {user.login?.username ?? "N/A"}");
            Console.WriteLine($"Şifre: {user.login?.password ?? "N/A"}");
            Console.WriteLine($"Uyruk: {user.nat ?? "N/A"}");

            Console.WriteLine("Konum Bilgileri:");
            Console.WriteLine($"  Sokak: {user.Location?.Street?.number.ToString() ?? "N/A"} {user.Location?.Street?.name ?? "N/A"}");
            Console.WriteLine($"  Şehir: {user.Location?.city ?? "N/A"}");
            Console.WriteLine($"  Eyalet: {user.Location?.state ?? "N/A"}");
            Console.WriteLine($"  Ülke: {user.Location?.country ?? "N/A"}");
            Console.WriteLine($"  Posta Kodu: {user.Location?.postcode?.ToString() ?? "N/A"}"); // postcode string'e çevrildi

            Console.WriteLine("Diğer Bilgiler:");
            Console.WriteLine($"  Kayıt Tarihi: {user.Registered?.date ?? "N/A"}");
            Console.WriteLine($"  Ehliyet Tarihi: {user.Dob?.date ?? "N/A"}");
            Console.WriteLine($"  ID Türü: {user.Id?.name ?? "N/A"}");
            Console.WriteLine($"  ID Değeri: {user.Id?.value ?? "N/A"}");
            Console.WriteLine("---------------------------------------------");

            using (var conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    //Console.WriteLine("Veritabanı bağlantısı User, Location, Login için başarıyla açıldı.");

                    Guid locationId = Guid.NewGuid();
                    Guid loginId = Guid.NewGuid();
                    Guid userId = Guid.NewGuid();

                    // 1. Location tablosuna kaydet
                    if (user.Location != null)
                    {
                        Console.WriteLine("Location verisi kaydediliyor...");
                        using (var cmd = new NpgsqlCommand(
                            "INSERT INTO \"location\" (\"LocationId\", \"Street_number\", \"Street_name\", \"city\", \"state\", \"country\", \"postcode\", \"Coordinates_latitude\", \"Coordinates_longitude\", \"Timezone_offset\", \"Timezone_description\") VALUES (@LocationId, @Street_number, @Street_name, @city, @state, @country, @postcode, @Coordinates_latitude, @Coordinates_longitude, @Timezone_offset, @Timezone_description)", conn))
                        {
                            cmd.Parameters.AddWithValue("LocationId", locationId);
                            cmd.Parameters.AddWithValue("Street_number", (object)user.Location.Street?.number ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("Street_name", (object)user.Location.Street?.name ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("city", (object)user.Location.city ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("state", (object)user.Location.state ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("country", (object)user.Location.country ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("postcode", (object)user.Location.postcode?.ToString() ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("Coordinates_latitude", (object)user.Location.Coordinates?.latitude ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("Coordinates_longitude", (object)user.Location.Coordinates?.longitude ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("Timezone_offset", (object)user.Location.Timezone?.offset ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("Timezone_description", (object)user.Location.Timezone?.description ?? DBNull.Value);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected > 0) Console.WriteLine("Location verisi veritabanına başarıyla kaydedildi.");
                            else Console.WriteLine("Uyarı: Location verisi kaydedilemedi (0 satır etkilendi).");
                        }
                    }

                    // 2. Login tablosuna kaydet
                    if (user.login != null)
                    {
                        Console.WriteLine("Login verisi kaydediliyor...");
                        using (var cmd = new NpgsqlCommand(
                            "INSERT INTO \"login\" (\"LoginId\", \"username\", \"password\", \"salt\", \"md5\", \"sha1\", \"sha256\", \"uuid\") VALUES (@LoginId, @username, @password, @salt, @md5, @sha1, @sha256, @uuid)", conn))
                        {
                            cmd.Parameters.AddWithValue("LoginId", loginId);
                            cmd.Parameters.AddWithValue("username", (object)user.login.username ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("password", (object)user.login.password ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("salt", (object)user.login.salt ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("md5", (object)user.login.md5 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("sha1", (object)user.login.sha1 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("sha256", (object)user.login.sha256 ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("uuid", (object)user.login.uuid ?? DBNull.Value);
                            int rowsAffected = await cmd.ExecuteNonQueryAsync();
                            if (rowsAffected > 0) Console.WriteLine("Login verisi veritabanına başarıyla kaydedildi.");
                            else Console.WriteLine("Uyarı: Login verisi kaydedilemedi (0 satır etkilendi).");
                        }
                    }

                    // 3. Ana User tablosuna kaydet
                    Console.WriteLine("User ana verisi kaydediliyor...");
                    using (var cmd = new NpgsqlCommand(
                        "INSERT INTO \"User\" (\"UserId\", \"gender\", \"email\", \"nat\", \"phone\", \"cell\", \"Registered_date\", \"Registered_age\", \"Dob_date\", \"Dob_age\", \"Id_name\", \"Id_value\", \"Name_title\", \"Name_first\", \"Name_last\", \"Picture_large\", \"Picture_medium\", \"Picture_thumbnail\", \"LocationId\", \"LoginId\") VALUES (@UserId, @gender, @email, @nat, @phone, @cell, @Registered_date, @Registered_age, @Dob_date, @Dob_age, @Id_name, @Id_value, @Name_title, @Name_first, @Name_last, @Picture_large, @Picture_medium, @Picture_thumbnail, @LocationId, @LoginId)", conn))
                    {
                        cmd.Parameters.AddWithValue("UserId", userId);
                        cmd.Parameters.AddWithValue("gender", (object)user.gender ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("email", (object)user.email ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("nat", (object)user.nat ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("phone", (object)user.phone ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("cell", (object)user.cell ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Registered_date", (object)user.Registered?.date ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Registered_age", (object)user.Registered?.age ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Dob_date", (object)user.Dob?.date ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Dob_age", (object)user.Dob?.age ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Id_name", (object)user.Id?.name ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Id_value", (object)user.Id?.value ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Name_title", (object)user.Name?.title ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Name_first", (object)user.Name?.first ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Name_last", (object)user.Name?.last ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Picture_large", (object)user.Picture?.large ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Picture_medium", (object)user.Picture?.medium ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("Picture_thumbnail", (object)user.Picture?.thumbnail ?? DBNull.Value);

                        cmd.Parameters.AddWithValue("LocationId", (object)locationId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("LoginId", (object)loginId ?? DBNull.Value);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Kullanıcı {user.Name?.first} {user.Name?.last} veritabanına başarıyla kaydedildi ({rowsAffected} satır etkilendi).");
                        }
                        else
                        {
                            Console.WriteLine($"Uyarı: Kullanıcı {user.Name?.first} {user.Name?.last} veritabanına kaydedilemedi (0 satır etkilendi).");
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    Console.WriteLine($"Hata: Veritabanına kullanıcı kaydederken Npgsql hatası oluştu: {ex.Message}");
                    Console.WriteLine($"SQL State: {ex.SqlState}");
                    Console.WriteLine($"Detay: {ex.StackTrace}");
                    Console.WriteLine("Lütfen User, Location ve Login tablolarınızın doğru yapılandırıldığından, foreign key'lerin uyumlu olduğundan emin olun.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Hata: Veritabanına kullanıcı kaydederken beklenmeyen bir hata oluştu: {ex.Message}");
                    Console.WriteLine($"Hata Detayı: {ex.InnerException?.Message ?? "Yok"}");
                    Console.WriteLine($"Hata Tipi: {ex.GetType().Name}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                }
            }
        }
    }
}