using Newtonsoft.Json;

namespace apideneme.models2
{
    public class RandomUserApiResponse //ham veriyi anlaşılır yapar. yanıt demektir
    {
        [JsonProperty("results")]
        public List<User> User { get; set; } //result adında liste var results ise bunun içinde bir sınıftır.
        public Info info { get; set; } //ayrıyetten info sınıfı da var
        
    }
}
