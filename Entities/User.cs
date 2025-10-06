using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace core8_astro_informix.Entities
{
    [Table("users")]
    public class User {

        [Key]
        [Column("id")]
        public int Id {get; set;}
// [Column("LASTNAME", TypeName = "VARCHAR(50)")]
        [Column("firstname")]        
        public string FirstName {get; set;}

        [Column("lastname")]
        public string LastName {get; set;}

        [Column("email")]
        public string Email { get; set; }

        [Column("mobile")]
        public string Mobile { get; set; }

        [Column("username")]
        public string UserName {get; set;}

        [Column("password_hash")]
        public string Password_hash {get; set;}
        
        [Column("roles")]
        public string Roles { get; set; }

        [Column("isactivated")]
        public int IsActivated {get; set;}

        [Column("isblocked")]
        public int Isblocked {get; set;}

        [Column("mailtoken")]
        public int Mailtoken {get; set;}

        [Column("qrcoderul")]
        public string Qrcodeurl {get; set;}

        [Column("profilepic")]
        public string Profilepic {get; set;}

        [Column("secretkey")]
        public string Secretkey {get; set;}

        [Column("createdat")]
        public DateTime CreatedAt {get; set;}

        [Column("updatedat")]
        public DateTime UpdatedAt {get; set;}
    }
}