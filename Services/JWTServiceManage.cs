using AutoMapper.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;
using core8_astro_informix.Helpers;
using core8_astro_informix.Entities;
using IBM.Data.Db2;

namespace core8_astro_informix.Services
{
    public interface IJWTTokenServices
    {
        JWTTokens Authenticate(User users);
    }
    public class JWTServiceManage : IJWTTokenServices
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

         IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();
 
        public JWTServiceManage(IConfiguration configuration)
        {
            _connectionString = config.GetConnectionString("InformixConnection");            
            _configuration = configuration;
        }

        public JWTTokens Authenticate(User users)
        {
            using (var connection = new DB2Connection(_connectionString))
            {
                connection.Open();
                var sql = "SELECT username FROM rey.users WHERE username = @username and password_hash = @password FETCH FIRST 1 ROW ONLY";                    
                using (var command = new DB2Command(sql, connection))
                {
                    command.Parameters.Add(new DB2Parameter("@username", users.UserName));
                    command.Parameters.Add(new DB2Parameter("@password", users.Password_hash));
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader.GetString(reader.GetOrdinal("username")) is null) {
                                return null;                             
                            }
                        }
                    }
                }
            }

            var tokenhandler = new JwtSecurityTokenHandler();
            var tkey = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var ToeknDescp = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, users.UserName)
                }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tkey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenhandler.CreateToken(ToeknDescp);
 
            return new JWTTokens { Token = tokenhandler.WriteToken(token) };
 
        }
    }    
    
}