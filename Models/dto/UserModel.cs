using System;

namespace core8_astro_informix.Models.dto
{
    public class UserModel {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Mobile { get; set; }
        public string Roles { get; set; }
        public string Profilepic { get; set; }
        public string Secretkey { get; set; }
        public int Isactivated { get; set; }        
        public int Isblocked { get; set; }
        public string Qrcodeurl { get; set; }
        public int Mailtoken { get; set; }
        public DateTime UpdatedAt { get; set; } = default;
    }    
}