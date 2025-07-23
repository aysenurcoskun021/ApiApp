
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ApiConsoleApp.models2
{
    [Table("login")] // Küçük harfli tabloya işaret ediyoruz
    public class Login
    {
        [Key]
        public Guid LoginId { get; set; }

        public string username { get; set; }
        public string password { get; set; }
        public string salt { get; set; }
        public string md5 { get; set; }
        public string sha1 { get; set; }
        public string sha256 { get; set; }
        public string uuid { get; set; }
        public ICollection<User> User { get; set; }//ekledimm
      
    }
}

