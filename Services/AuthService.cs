using System;
using System.Data;
using System.IO;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using core8_astro_informix.Entities;
using core8_astro_informix.Helpers;
using core8_astro_informix.Models.dto;
using IBM.Data.Db2;
//    "InformixConnection": "Host=localhost;Service=9088;Protocol=onsoctcp;Server=informix;Database=core8;User Id=rey;Password=rey;DB_LOCALE=en_US.819;CLIENT_LOCALE=en_US.819;"


namespace core8_astro_informix.Services
{    
    public interface IAuthService {
        Task<User> SignupUser(User userdata, string passwd);
        Task<User> SigninUser(string usrname, string pwd);
    }

    public class AuthService : IAuthService
    {
        private readonly AppSettings _appSettings;
        private readonly string _connectionString;

         IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();

        public AuthService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _connectionString = config.GetConnectionString("InformixConnection");            
        }



        public async Task<User> SignupUser(User userdata, string passwd)
        {
            using (var connection = new DB2Connection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "SELECT email FROM rey.users WHERE email = @email FETCH FIRST 1 ROW ONLY";                    
                using (var command = new DB2Command(sql, connection))
                {
                    command.Parameters.Add(new DB2Parameter("@email", userdata.Email));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (reader.GetString(reader.GetOrdinal("email")) is not null) {
                                throw new AppException("Email Address is already taken...");
                            }
                        }
                    }
                }
            }

            using (var connection = new DB2Connection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "SELECT username FROM rey.users WHERE username = @username FETCH FIRST 1 ROW ONLY";                    
                using (var command = new DB2Command(sql, connection))
                {
                    command.Parameters.Add(new DB2Parameter("@username", userdata.UserName));
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (reader.GetString(reader.GetOrdinal("username")) is not null) {
                                throw new AppException("Username is already taken...");                                
                            }
                        }
                    }
                }
            }
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var xkey = config["Jwt:Key"];
            var key = Encoding.ASCII.GetBytes(xkey);

            // // CREATE SECRET KEY FOR USER TOKEN===============
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, userdata.Email)
                }),
                // Expires = DateTime.UtcNow.AddDays(7),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var secret = tokenHandler.CreateToken(tokenDescriptor);
            var secretkey = tokenHandler.WriteToken(secret);

            userdata.Secretkey = secretkey.ToUpper();             
            userdata.Password_hash = BCrypt.Net.BCrypt.HashPassword(passwd);
            userdata.Qrcodeurl = "";

            try {
                using (var connection = new DB2Connection(_connectionString))
                {
                    await connection.OpenAsync();
                    string sql = "INSERT INTO rey.users(firstname,lastname,email,mobile,username,password_hash,roles,isactivated,isblocked,mailtoken,qrcodeurl,profiepic,secretkey) VALUES (@firstname, @lastname, @email, @mobile, @username, @password, @roles, @isactivated, @isblocked, @qrcodeurl, @mailtoken, @profilepic, @secretkey)";
                    using (var cmd = new DB2Command(sql, connection))
                    {
                        cmd.Parameters.Add(new DB2Parameter("@firstname", userdata.FirstName));
                        cmd.Parameters.Add(new DB2Parameter("@lastname", userdata.LastName));
                        cmd.Parameters.Add(new DB2Parameter("@email", userdata.Email));
                        cmd.Parameters.Add(new DB2Parameter("@mobile", userdata.Mobile));
                        cmd.Parameters.Add(new DB2Parameter("@username", userdata.UserName));
                        cmd.Parameters.Add(new DB2Parameter("@password", userdata.Password_hash));
                        cmd.Parameters.Add(new DB2Parameter("@roles", userdata.Roles));
                        cmd.Parameters.Add(new DB2Parameter("@isactivated", userdata.IsActivated));
                        cmd.Parameters.Add(new DB2Parameter("@isblocked", userdata.Isblocked));
                        cmd.Parameters.Add(new DB2Parameter("@qrcodeurl", null));
                        cmd.Parameters.Add(new DB2Parameter("@mailtoken", userdata.Mailtoken));
                        cmd.Parameters.Add(new DB2Parameter("@profilepic", userdata.Profilepic));
                        cmd.Parameters.Add(new DB2Parameter("@secretkey", userdata.Secretkey));
                        cmd.ExecuteNonQuery();
                    }
                }
                return userdata;
            } catch(Exception ex) {
                throw new AppException(ex.Message);
            }

        }

        public async Task<User> SigninUser(string usrname, string pwd)
        {
                var xuser = new User();
                using (var connection = new DB2Connection(_connectionString))
                {
                    await connection.OpenAsync();
                    var sql = "SELECT * FROM rey.users WHERE username = @username FETCH FIRST 1 ROW ONLY";                    
                    using (var command = new DB2Command(sql, connection))
                    {
                        command.Parameters.Add(new DB2Parameter("@username", usrname));
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                              if (reader.GetString(reader.GetOrdinal("username")) is null) {
                                if (!BCrypt.Net.BCrypt.Verify(pwd, reader.GetString(reader.GetOrdinal("password_hash")))) {
                                    throw new AppException("Incorrect Password...");
                                }
                                if (reader.GetInt32(reader.GetOrdinal("isactivated")) == 1 ) {
                                    throw new AppException("Please activate your account, check your email client inbox and click or tap the Activate button.");
                                }
                              }
                                xuser.Id = reader.GetInt32(reader.GetOrdinal("id"));
                                xuser.FirstName = reader.GetString(reader.GetOrdinal("firstname"));
                                xuser.LastName = reader.GetString(reader.GetOrdinal("lastname"));
                                xuser.Email = reader.GetString(reader.GetOrdinal("email"));


                            }
                        }
                    }
                }
                return xuser;
        }


    }
}
