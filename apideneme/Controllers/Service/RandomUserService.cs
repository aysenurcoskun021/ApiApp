using Newtonsoft.Json; //metinleri çevirir dilden dile json<->c#
using apideneme.models2;
using apideneme.Controllers.Data;

namespace apideneme.Controllers.Service;

public class RandomUserService
{
    public readonly HttpClient _httpClient; //bir kez atanabileceği anlamında readonly.
    private readonly ApplicationDbContext _context;

    public RandomUserService(HttpClient httpClient, ApplicationDbContext context)
    {
        _httpClient = httpClient; //_httpclient özel alan anlamına gelir.
        _context = context; //veritabanına kayı yapar okur bunun sayesinde
                            //dışarıdan gelen verileri bu özel alanlara atama yapılır.
    }

    //task eşzamansız işlemin sonucu tamamlandığında gösterir
    public async Task<RandomUserApiResponse> GetRandomUserAsync() //dışapiden veri çekme
    {
        try //hata olabilecek hata buraya yazılır
        {
            //bağlantıya istek atar geleni string alır. 
            //eşzamansız işlemin tamamlanmasını bekler. internetli olanlarda await kullanırız ki donmasın
            var response = await _httpClient.GetStringAsync("https://randomuser.me/api");
            //jsonu c#diline çevirir. 
            return JsonConvert.DeserializeObject<RandomUserApiResponse>(response);
        }
        catch (Exception ex) //hata olursa buraya atlar
        {
            throw; //controllera gönderir. o da kullanıcıya açıklar.
        }
    }

    //public ApplicationDbContext Get_context() //dbcontexte erişim sağlanır
    //{
    //    return _context; //veritabanı nesnesini döndürür
    //}
    //bool sadece true false alır.başarılı olup olmadığı ile ilgilidir.
    public async Task<bool> CreateDataInPostgreSSQL()//(ApplicationDbContext _context)
    {
        var data = await GetRandomUserAsync();

        if (data == null) //apiden veri gelip gelmediği
        {
            Console.WriteLine("API'den veri alınamadı.");
            return false;
        }
        try
        {
            if (data.info != null) //veri geliyorsa
            {
                _context.Info.Add(data.info); //infoyu ekler
            }


            if (data.User != null && data.User.Any()) //result listesi var mı? ve içi dolu mu?
            {
                foreach (var result in data.User) //liste boş değilse
                {


                    _context.User.Add(result); //döngüye result girer. eklenir
                }
            }
            else
            {
                Console.WriteLine("API yanıtında Result verisi bulunamadı.");
            }
            await _context.SaveChangesAsync(); //eşzamansız olarak olur bunlar. ef core belleğinde işaretlenir ve kaydedilir.
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

    public async Task<bool> GetDataInMSSSQL()//(ApplicationDbContext _context)
    {
        var data = await GetRandomUserAsync();

        if (data?.User == null || data.User.Count == 0)
            return false;

        foreach (var result in data.User)
        {
            _context.Add(result);
        }
        await _context.SaveChangesAsync();
        return true;
    }
}