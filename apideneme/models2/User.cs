using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace apideneme.models2
{
    public class User
    {
        public Guid UserId { get; set; }
        public string gender { get; set; }
        public string email { get; set; }
        public string nat { get; set; }
        public string phone { get; set; }
        public string cell { get; set; }
        public Registered Registered { get; set; }
        public Dob Dob { get; set; }
        public Id Id { get; set; }
        public Name Name { get; set; }
        public Picture Picture { get; set; }
        
        [ForeignKey("LocationId")]
        public Guid LocationId { get; set; }
        public Location location { get; set; }//ekleidm
        
        [ForeignKey("LoginId")]
        public Guid LoginId { get; set; }    // Foreign Key ID,
        public Login login { get; set; }//ekledim
       

    }

    [Owned]
    public class Registered
    {
        public string date { get; set; }
        public string age { get; set; }
    }

    [Owned]
    public class Dob
    {
        public string date { get; set; }
        public string age { get; set; }

    }

    [Owned]
    public class Id
    {
        public string name { get; set; }
        public string? value { get; set; }
    }

    [Owned]
    public class Name
    {
        public string title { get; set; }
        public string first { get; set; }
        public string last { get; set; }
    }

    [Owned]
    public class Picture
    {
        public string large { get; set; }
        public string medium { get; set; }
        public string thumbnail { get; set; }
    }
}
