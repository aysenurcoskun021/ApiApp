using System.Collections.Generic;
using System.Text.Json.Serialization; // JsonPropertyName için

namespace ApiConsoleApp.models2 // Namespace'inizin models2 olduğundan emin olun
{
    public class RandomUserApiResponse
    {
        // API'den gelen JSON'daki "results" anahtarını bu özelliğe eşleştirmek için
        [JsonPropertyName("results")] 
        public List<User> results { get; set; } // Özelliğin adını "results" olarak değiştirin

        public Info info { get; set; }
    }
}