namespace core8_astro_informix.Models.dto
{
    public class ForgotPassword {        
        public int Mailtoken {get; set;}
        public string Password_hash {get; set;}
    }
}