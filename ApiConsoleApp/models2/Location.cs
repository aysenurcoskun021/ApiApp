using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations; //ekledim
using System.ComponentModel.DataAnnotations.Schema; //ekledim

namespace ApiConsoleApp.models2
{
    [Table("location")] //ekledim
    public class Location
    {
        [Key]//ekledim
        public Guid LocationId { get; set; }
        public Street Street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }

        [JsonConverter(typeof(ToStringConverter))]
        public string postcode { get; set; }
        public Coordinates Coordinates { get; set; }
        public Timezone Timezone { get; set; }

        public ICollection<User> User { get; set; }//ekledimm


    }

    [Owned]
    public class Street
    {
        public int number { get; set; }
        public string name { get; set; }
    }

    
    [Owned]
    public class Coordinates
    {
        public string latitude { get; set; }
        public string longitude { get; set; }
    }

    
    [Owned]
    public class Timezone
    {
        public string offset { get; set; }
        public string description { get; set; }
    }
 

    public class ToStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true; // her tip için çalışsın
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value?.ToString();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }
    }
}