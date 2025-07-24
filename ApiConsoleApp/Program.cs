using System;
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
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("Hata: 'DefaultConnection' bağlantı dizesi bulunamadı.");
                return;
            }

            Console.WriteLine("API'den kullanıcı verileri çekiliyor...");

            try
            {
                var apiResponse = await FetchApiData();
                if (apiResponse != null)
                {
                    if (apiResponse.info != null)
                    {
                        PrintInfo(apiResponse.info);
                        await SaveInfoToDatabase(apiResponse.info, connectionString);
                    }

                    if (apiResponse.results?.Any() == true)
                    {
                        foreach (var user in apiResponse.results)
                        {
                            PrintUserInfo(user);
                            await SaveUserToDatabase(user, connectionString);
                        }
                        Console.WriteLine("Tüm kullanıcılar veritabanına kaydedildi.");
                    }
                }
                else Console.WriteLine("API yanıtı boş döndü.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Beklenmedik hata: {ex.Message}");
            }

            Console.WriteLine("Tüm işlemler tamamlandı. Çıkmak için bir tuşa basın...");
            Console.ReadKey();
        }

        static async Task<RandomUserApiResponse> FetchApiData()
        {
            using var client = new HttpClient();
            var jsonString = await client.GetStringAsync("https://randomuser.me/api?results=1");
            return JsonConvert.DeserializeObject<RandomUserApiResponse>(jsonString);
        }

        static async Task SaveInfoToDatabase(Info info, string connectionString)
        {
            var query = "INSERT INTO \"Info\" (\"seed\", \"results\", \"page\", \"version\") VALUES (@seed, @results, @page, @version)";
            var parameters = new[]
            {
                new NpgsqlParameter("seed", (object)info.seed ?? DBNull.Value),
                new NpgsqlParameter("results", info.results),
                new NpgsqlParameter("page", info.page),
                new NpgsqlParameter("version", (object)info.version ?? DBNull.Value)
            };
            var rows = await ExecuteNonQueryAsync(query, parameters, connectionString);
            Console.WriteLine(rows > 0 ? "Info verisi kaydedildi." : "Info verisi kaydedilemedi.");
        }

        static async Task SaveUserToDatabase(User user, string connectionString)
        {
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            var locationId = Guid.NewGuid();
            var loginId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            if (user.Location != null)
                await InsertLocation(user.Location, locationId, conn);

            if (user.login != null)
                await InsertLogin(user.login, loginId, conn);

            var query = @"INSERT INTO ""User""
                (""UserId"", ""gender"", ""email"", ""nat"", ""phone"", ""cell"", ""Registered_date"", ""Registered_age"",
                 ""Dob_date"", ""Dob_age"", ""Id_name"", ""Id_value"", ""Name_title"", ""Name_first"", ""Name_last"",
                 ""Picture_large"", ""Picture_medium"", ""Picture_thumbnail"", ""LocationId"", ""LoginId"")
                VALUES (@UserId, @gender, @email, @nat, @phone, @cell, @Registered_date, @Registered_age,
                        @Dob_date, @Dob_age, @Id_name, @Id_value, @Name_title, @Name_first, @Name_last,
                        @Picture_large, @Picture_medium, @Picture_thumbnail, @LocationId, @LoginId)";

            var parameters = new[]
            {
                new NpgsqlParameter("UserId", userId),
                new NpgsqlParameter("gender", (object)user.gender ?? DBNull.Value),
                new NpgsqlParameter("email", (object)user.email ?? DBNull.Value),
                new NpgsqlParameter("nat", (object)user.nat ?? DBNull.Value),
                new NpgsqlParameter("phone", (object)user.phone ?? DBNull.Value),
                new NpgsqlParameter("cell", (object)user.cell ?? DBNull.Value),
                new NpgsqlParameter("Registered_date", (object)user.Registered?.date ?? DBNull.Value),
                new NpgsqlParameter("Registered_age", (object)user.Registered?.age ?? DBNull.Value),
                new NpgsqlParameter("Dob_date", (object)user.Dob?.date ?? DBNull.Value),
                new NpgsqlParameter("Dob_age", (object)user.Dob?.age ?? DBNull.Value),
                new NpgsqlParameter("Id_name", (object)user.Id?.name ?? DBNull.Value),
                new NpgsqlParameter("Id_value", (object)user.Id?.value ?? DBNull.Value),
                new NpgsqlParameter("Name_title", (object)user.Name?.title ?? DBNull.Value),
                new NpgsqlParameter("Name_first", (object)user.Name?.first ?? DBNull.Value),
                new NpgsqlParameter("Name_last", (object)user.Name?.last ?? DBNull.Value),
                new NpgsqlParameter("Picture_large", (object)user.Picture?.large ?? DBNull.Value),
                new NpgsqlParameter("Picture_medium", (object)user.Picture?.medium ?? DBNull.Value),
                new NpgsqlParameter("Picture_thumbnail", (object)user.Picture?.thumbnail ?? DBNull.Value),
                new NpgsqlParameter("LocationId", locationId),
                new NpgsqlParameter("LoginId", loginId)
            };

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddRange(parameters);
            var rows = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine(rows > 0 ? $"Kullanıcı {user.Name?.first} {user.Name?.last} kaydedildi." : "Kullanıcı kaydedilemedi.");
        }

        static async Task InsertLocation(Location loc, Guid locationId, NpgsqlConnection conn)
        {
            var query = @"INSERT INTO ""location"" 
                (""LocationId"", ""Street_number"", ""Street_name"", ""city"", ""state"", ""country"", ""postcode"",
                 ""Coordinates_latitude"", ""Coordinates_longitude"", ""Timezone_offset"", ""Timezone_description"")
                VALUES (@LocationId, @Street_number, @Street_name, @city, @state, @country, @postcode,
                        @Coordinates_latitude, @Coordinates_longitude, @Timezone_offset, @Timezone_description)";

            var parameters = new[]
            {
                new NpgsqlParameter("LocationId", locationId),
                new NpgsqlParameter("Street_number", (object)loc.Street?.number ?? DBNull.Value),
                new NpgsqlParameter("Street_name", (object)loc.Street?.name ?? DBNull.Value),
                new NpgsqlParameter("city", (object)loc.city ?? DBNull.Value),
                new NpgsqlParameter("state", (object)loc.state ?? DBNull.Value),
                new NpgsqlParameter("country", (object)loc.country ?? DBNull.Value),
                new NpgsqlParameter("postcode", (object)loc.postcode?.ToString() ?? DBNull.Value),
                new NpgsqlParameter("Coordinates_latitude", (object)loc.Coordinates?.latitude ?? DBNull.Value),
                new NpgsqlParameter("Coordinates_longitude", (object)loc.Coordinates?.longitude ?? DBNull.Value),
                new NpgsqlParameter("Timezone_offset", (object)loc.Timezone?.offset ?? DBNull.Value),
                new NpgsqlParameter("Timezone_description", (object)loc.Timezone?.description ?? DBNull.Value)
            };

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddRange(parameters);
            await cmd.ExecuteNonQueryAsync();
        }

        static async Task InsertLogin(Login login, Guid loginId, NpgsqlConnection conn)
        {
            var query = @"INSERT INTO ""login"" 
                (""LoginId"", ""username"", ""password"", ""salt"", ""md5"", ""sha1"", ""sha256"", ""uuid"")
                VALUES (@LoginId, @username, @password, @salt, @md5, @sha1, @sha256, @uuid)";

            var parameters = new[]
            {
                new NpgsqlParameter("LoginId", loginId),
                new NpgsqlParameter("username", (object)login.username ?? DBNull.Value),
                new NpgsqlParameter("password", (object)login.password ?? DBNull.Value),
                new NpgsqlParameter("salt", (object)login.salt ?? DBNull.Value),
                new NpgsqlParameter("md5", (object)login.md5 ?? DBNull.Value),
                new NpgsqlParameter("sha1", (object)login.sha1 ?? DBNull.Value),
                new NpgsqlParameter("sha256", (object)login.sha256 ?? DBNull.Value),
                new NpgsqlParameter("uuid", (object)login.uuid ?? DBNull.Value)
            };

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddRange(parameters);
            await cmd.ExecuteNonQueryAsync();
        }

        static async Task<int> ExecuteNonQueryAsync(string query, NpgsqlParameter[] parameters, string connectionString)
        {
            using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteNonQueryAsync();
        }

        static void PrintUserInfo(User user)
        {
            Console.WriteLine("\n--- Kullanıcı Bilgileri ---");
            Console.WriteLine($"Ad Soyad: {user.Name?.title} {user.Name?.first} {user.Name?.last}");
            Console.WriteLine($"E-posta: {user.email}");
            Console.WriteLine($"Cinsiyet: {user.gender}");
            Console.WriteLine($"Yaş: {user.Dob?.age}");
            Console.WriteLine($"Telefon: {user.phone}");
            Console.WriteLine($"Cep Telefonu: {user.cell}");
            Console.WriteLine($"Kullanıcı Adı: {user.login?.username}");
            Console.WriteLine($"Şifre: {user.login?.password}");
            Console.WriteLine($"Uyruk: {user.nat}");
            Console.WriteLine("Konum:");
            Console.WriteLine($"  Sokak: {user.Location?.Street?.number} {user.Location?.Street?.name}");
            Console.WriteLine($"  Şehir: {user.Location?.city}");
            Console.WriteLine($"  Eyalet: {user.Location?.state}");
            Console.WriteLine($"  Ülke: {user.Location?.country}");
            Console.WriteLine($"  Posta Kodu: {user.Location?.postcode}");
            Console.WriteLine($"Kayıt Tarihi: {user.Registered?.date}");
            Console.WriteLine($"Doğum Tarihi: {user.Dob?.date}");
            Console.WriteLine($"ID Türü: {user.Id?.name}");
            Console.WriteLine($"ID Değeri: {user.Id?.value}");
            Console.WriteLine("---------------------------");
        }

        static void PrintInfo(Info info)
        {
            Console.WriteLine("\n--- Info Bilgileri ---");
            Console.WriteLine($"Seed: {info.seed}");
            Console.WriteLine($"Results: {info.results}");
            Console.WriteLine($"Page: {info.page}");
            Console.WriteLine($"Version: {info.version}");
            Console.WriteLine("----------------------");
        }
    }
}
