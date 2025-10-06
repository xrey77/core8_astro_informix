using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace core8_astro_informix.Models.dto
{
  public class UserUpdate
    {        
        [Required]
        public string Firstname { get; set; }

        [Required]
        public string Lastname { get; set; }
        public string Password { get; set; }
        public string Mobile { get; set; }
        public DateTime UpdatedAt { get; set; }
    }    
}