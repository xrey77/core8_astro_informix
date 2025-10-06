using System.ComponentModel.DataAnnotations;

namespace core8_astro_informix.Models.dto
{
  public class UserRegister
    {        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Username { get; set; }
        public string Password_hash { get; set; }
        public string Userpic { get; set; }
        public string Roles { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }    
}