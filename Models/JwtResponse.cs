namespace core8_astro_informix.Models
{
    public class JwtResponse {
        public string token { get; set; }
        public string user_name { get; set; }
        public int expires_in { get; set; }
    }
}