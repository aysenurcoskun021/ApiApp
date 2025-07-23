using apideneme.Controllers.Data;
using Microsoft.AspNetCore.Mvc;
using apideneme.models2;
using apideneme.Controllers.Service;
using Microsoft.EntityFrameworkCore;

//web istekleri ve yapılacak işlemleri belirler.

namespace apideneme.Controllers
{
    public class CreateController : Controller //temel controllerdan miras alıyor. asp.net core'un
    {
        public IActionResult Index() //genel sonuç işlemi ve ana eylem metodu
        {
            return View();
        }
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly RandomUserService _RandomUserService;

        public CreateController(ApplicationDbContext context, HttpClient httpClient, RandomUserService RandomUserService)
        {
            _context = context;
            _httpClient = httpClient;
            _RandomUserService = RandomUserService;
        }

        [HttpGet("All")]
        public async Task<ActionResult<RandomUserApiResponse>> GetAllRandomUserService() //randomuserapi'den kullanıcıları çeker
        {
            return await _RandomUserService.GetRandomUserAsync();
        }

        [HttpGet("Create")]
        public async Task<bool> CreateDataInPostgreSSQL()
        {
            var apiResponse = await _RandomUserService.GetRandomUserAsync(); // API yanıtını al

            if (apiResponse?.User == null || !apiResponse.User.Any())
            {
                Console.WriteLine("API'den kullanıcı verisi alınamadı.");
                return false;
            }

            try
            {
                // API'den gelen her 'result' aslında bir 'User' ise
                foreach (var user in apiResponse.User)
                {
                    // Entity Framework Core, User'ın içindeki Location ve Login gibi
                    // [Owned] olarak işaretlenmiş veya ilişkili (configured) varlıkları
                    // User ile birlikte otomatik olarak ekleyecektir.
                    _context.User.Add(user); // Kullanıcıları User DbSet'ine ekle
                }
                
                // Eğer "Info" da ayrı bir DbSet ise ve eklenmesi gerekiyorsa:
                if (apiResponse.info != null)
                {
                    _context.Info.Add(apiResponse.info);
                }
               

                await _context.SaveChangesAsync(); // Tüm değişiklikleri tek seferde kaydet
                Console.WriteLine("Veriler PostgreSQL'e başarıyla eklendi.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Veri PostgreSQL'e eklenirken hata oluştu: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        

        [HttpGet("GetAllUsersFromDb")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsersFromDb()
        {
            // Eğer Result DbSet'i yoksa veya null ise 404 döndür
            if (_context.User == null)
            {
                return NotFound("VERİ TABANI YOK!");
            }

            try
            {
                // Tüm Result verilerini ve ilişkili diğer tabloları çek
                var allResults = await _context.User

                                             .Include(r => r.Name)
                                             .Include(r => r.Registered)
                                             .Include(r => r.Dob)
                                             .Include(u => u.Id)
                                             .Include(u => u.Picture)
                                             .Include(u => u.location)
                                             .Include(u => u.login)

                                             .ToListAsync();



                if (!allResults.Any())
                {
                    return NotFound("VERİ TABANI BOŞ!");
                }

                return Ok(allResults); // Verileri JSON formatında döndür
            }
            catch (Exception ex)
            {
                // Bir hata oluşursa, detaylı bir hata mesajı döner
                return StatusCode(500, $"Veri alınırken bir hata oluştu!");
            }
        }
    }
}