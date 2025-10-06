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
using IBM.Data.Db2;

namespace core8_astro_informix.Services
{
    public interface IUserService {
        Task<List<User>> GetAll();
        User GetById(int id);
        void UpdateProfile(User user);
        void Delete(int id);
        void ActivateMfa(int id, bool opt, string qrcode_url);
        void UpdatePicture(int id, string file);
        void UpdatePassword(User user, string password = null);
        int EmailToken(int etoken);
        Task<int> SendEmailToken(string email);
        void ActivateUser(int id);
        Task<bool> ChangePassword(User userParam);
    }

    public class UserService : IUserService
    {
        private readonly string _connectionString;
        private readonly AppSettings _appSettings;

         IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();

        public UserService(IOptions<AppSettings> appSettings)
        {
            _connectionString = config.GetConnectionString("InformixConnection");            
            _appSettings = appSettings.Value;
        }

        public void Delete(int id)
        {
            string sql = "DELETE FROM rey.users WHERE id = @id";
            using (var connection = new DB2Connection(_connectionString))
            {
                connection.Open();
                try {
                    using (var cmd = new DB2Command(sql, connection))
                    {
                        cmd.Parameters.Add(new DB2Parameter("@id", id));
                        cmd.ExecuteNonQuery();
                    }
                } catch(Exception) {
                    throw new AppException("User not found");
                }
            }            
        }

        public async Task<List<User>> GetAll()
        {
            var users = new List<User>();
            string sql = $"SELECT id,firstname,lastname,email,mobile,username,roles,isactivated,isblocked,profilepic FROM rey.users";
            await using (var connection = new DB2Connection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new DB2Command(sql, connection))
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(new User
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                FirstName = reader.GetString(reader.GetOrdinal("firstname")),
                                LastName = reader.GetString(reader.GetOrdinal("lastname")),
                                Email = reader.GetString(reader.GetOrdinal("email")),
                                Mobile = reader.GetString(reader.GetOrdinal("Mobile")),
                                UserName = reader.GetString(reader.GetOrdinal("username")),
                                Roles = reader.GetString(reader.GetOrdinal("roles")),
                                IsActivated = reader.GetInt32(reader.GetOrdinal("isactivated")),
                                Isblocked = reader.GetInt32(reader.GetOrdinal("isblocked")),
                                Profilepic = reader.GetString(reader.GetOrdinal("profilepic"))
                            });
                        }
                    }
                }                                
            }
            return users;
        }

        public User GetById(int id)
        {
            string sql = "SELECT id,firstname,lastname,email,mobile,username,roles,isactivated,isblocked,profilepic FROM rey.users WHERE id = @id";
            var user = new User();
            using (var connection = new DB2Connection(_connectionString))
            {
                connection.Open();
                try {
                 using (var command = new DB2Command(sql, connection))
                 {
                    command.Parameters.Add(new DB2Parameter("@id", id));
                    using (var reader =  command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user.Id = reader.GetInt32(reader.GetOrdinal("id"));
                            user.FirstName = reader.GetString(reader.GetOrdinal("firstname"));
                            user.LastName = reader.GetString(reader.GetOrdinal("lastname"));
                            user.Email = reader.GetString(reader.GetOrdinal("email"));
                            user.Mobile = reader.GetString(reader.GetOrdinal("mboile"));
                            user.UserName = reader.GetString(reader.GetOrdinal("username"));
                            user.Roles = reader.GetString(reader.GetOrdinal("roles"));
                            user.IsActivated = reader.GetInt32(reader.GetOrdinal("isactivated"));
                            user.Isblocked = reader.GetInt32(reader.GetOrdinal("isblocked"));
                            user.Profilepic = reader.GetString(reader.GetOrdinal("profilepic"));
                        }
                    }
                 }
                 return user;
                } catch(Exception) {
                    throw new AppException("User not found");
                }
            }
        }

        public void UpdateProfile(User userParam)
        {
            DateTime now = DateTime.Now;
            var sql = "UPDATE rey.users SET firstame = @fname, lastname = @lname, mobile = @mobile, updateat = @update WHERE id = @id";
            using (var connection = new DB2Connection(_connectionString))
            {
                connection.Open();
                try {
                    using (var cmd = new DB2Command(sql, connection))
                    {
                        cmd.Parameters.Add(new DB2Parameter("@fname", userParam.FirstName));
                        cmd.Parameters.Add(new DB2Parameter("@lname", userParam.LastName));
                        cmd.Parameters.Add(new DB2Parameter("@mobile", userParam.Mobile));
                        cmd.Parameters.Add(new DB2Parameter("@update", now));
                        cmd.Parameters.Add(new DB2Parameter("@id", userParam.Id));
                        cmd.ExecuteNonQuery();
                    }
                } catch(Exception) {
                    throw new AppException("User not found");
                }                
            }  
        }

        public void UpdatePassword(User userParam, string password)
        {
            DateTime now = DateTime.Now;
            var sql = "UPDATE rey.users SET password_hash = @password, updateat = @update WHERE id = @id";
            using (var connection = new DB2Connection(_connectionString))
            {
                connection.Open();
                try {
                    var pwd = BCrypt.Net.BCrypt.HashPassword(userParam.Password_hash);
                    using (var cmd = new DB2Command(sql, connection))
                    {
                        cmd.Parameters.Add(new DB2Parameter("@password", pwd));
                        cmd.Parameters.Add(new DB2Parameter("@update", now));
                        cmd.Parameters.Add(new DB2Parameter("@id", userParam.Id));
                        cmd.ExecuteNonQuery();
                    }
                } catch(Exception) {
                    throw new AppException("User not found");
                }                
            }                                 
        }


        public void ActivateMfa(int id, bool opt, string qrcode_url)
        {
            DateTime now = DateTime.Now;
            var sql = "UPDATE rey.users SET qrcodeurl = @qrcode, updateat = @update WHERE id = @id";
            using (var connection = new DB2Connection(_connectionString))
            {
                connection.Open();
                try {
                    using (var cmd = new DB2Command(sql, connection))
                    {
                        if (opt == true ) {
                          cmd.Parameters.Add(new DB2Parameter("@qrucode", qrcode_url));
                        } else {
                          cmd.Parameters.Add(new DB2Parameter("@qrucode", null));
                        }
                        cmd.Parameters.Add(new DB2Parameter("@update", now));
                        cmd.Parameters.Add(new DB2Parameter("@id", id));
                        cmd.ExecuteNonQuery();
                    }
                } catch(Exception) {
                    throw new AppException("User not found");
                }                
            }                
        }
        

        public void UpdatePicture(int id, string file)
        {
            DateTime now = DateTime.Now;
            var sql = "UPDATE rey.users SET profilepic = @profilepic, updateat = @update WHERE id = @id";
            using (var connection = new DB2Connection(_connectionString))
            {
                connection.Open();
                try {
                    using (var cmd = new DB2Command(sql, connection))
                    {
                        cmd.Parameters.Add(new DB2Parameter("@profilepic", file));
                        cmd.Parameters.Add(new DB2Parameter("@update", now));
                        cmd.Parameters.Add(new DB2Parameter("@id", id));
                        cmd.ExecuteNonQuery();
                    }
                } catch(Exception) {
                    throw new AppException("User not found");
                }                
            }                               
        }

       public void ActivateUser(int id) 
       {
            DateTime now = DateTime.Now;
            using (var connection = new DB2Connection(_connectionString))
            {
                connection.Open();
                var sql = "SELECT id,firstname,lastname,email,mobile,roles,isactivated,isblocked,profilepic,mailtoken,secretkeyqrcoderul FROM rey.users WHERE id = @id FETCH FIRST 1 ROW ONLY";
                using (var command = new DB2Command(sql, connection))
                {
                    command.Parameters.Add(new DB2Parameter("@id", id));
                    using (var reader = command.ExecuteReader())
                    {
                        if ( reader.Read())
                        {
                            if (reader.GetInt32(reader.GetOrdinal("isblocked")) == 1) {
                                throw new AppException("Account has been blocked.");
                            }
                            if (reader.GetInt32(reader.GetOrdinal("isactivated")) == 1 ) {
                                throw new AppException("Account is alread activated.");
                            }
                        }
                    }
                    //UPDATE
                    var updateSql = "UPDATE rey.users SET isactivated = @activate WEHERE id = @id ";
                    using (var cmd = new DB2Command(updateSql, connection))
                    {
                        cmd.Parameters.Add(new DB2Parameter("@activate", 1));
                        cmd.Parameters.Add(new DB2Parameter("@update", now));
                        cmd.Parameters.Add(new DB2Parameter("@id", id));
                        cmd.ExecuteNonQuery();
                    }
                }
            }  
       }

        //CREATE MAILTOKEN AND SENT TO REGISTERED USER EMAIL
        public async Task<int> SendEmailToken(string email)
        {
            int etoken = 0;
            DateTime now = DateTime.Now;
            await using (var connection = new DB2Connection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "SELECT email,mailtoken FROM rey.users WHERE email = @email FETCH FIRST 1 ROW ONLY";
                await using (var command = new DB2Command(sql, connection))
                {
                    command.Parameters.Add(new DB2Parameter("@email", email));
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (reader.GetString(reader.GetOrdinal("email")) is null) {
                                throw new AppException("Email Address not found...");
                            }
                        }

                        etoken = EmailToken(reader.GetInt32(reader.GetOrdinal("mailtoken")));
                    }

                    //UPDATE
                    var updateSql = "UPDATE rey.users SET mailtoken = @token WEHERE email = @email ";
                    using (var cmd = new DB2Command(updateSql, connection))
                    {
                        cmd.Parameters.Add(new DB2Parameter("@token", etoken));
                        cmd.Parameters.Add(new DB2Parameter("@update", now));
                        cmd.Parameters.Add(new DB2Parameter("@email", email));
                        cmd.ExecuteNonQuery();
                    }
                }
            }  
            return etoken;
        }       

        //CREATE MAILTOKEN
        public int EmailToken(int etoken)
        {
            if (etoken == 0) {
                etoken = 1000;
            }
            int _min = etoken;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max);
        }

        public async Task<bool> ChangePassword(User userParam)
        {
            DateTime now = DateTime.Now;
            await using (var connection = new DB2Connection(_connectionString))
            {
                connection.Open();
                var sql = "SELECT * FROM rey.users WHERE username = @username FETCH FIRST 1 ROW ONLY";
                await using (var command = new DB2Command(sql, connection))
                {
                    command.Parameters.Add(new DB2Parameter("@username", userParam.UserName));
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (reader.GetString(reader.GetOrdinal("email")) is null) {
                                throw new AppException("Email Address not found...");
                            }

                            if (reader.GetInt32(reader.GetOrdinal("mailtoken")) == userParam.Mailtoken) {

                                var pwd = BCrypt.Net.BCrypt.HashPassword(userParam.Password_hash);
                                var updateSql = "UPDATE users SET mailtoken = @token, passowrd_hash = @password WEHERE username = @username ";
                                using (var cmd = new DB2Command(updateSql, connection))
                                {
                                    cmd.Parameters.Add(new DB2Parameter("@token", 0));
                                    cmd.Parameters.Add(new DB2Parameter("@password", pwd));
                                    cmd.Parameters.Add(new DB2Parameter("@update", now));
                                    cmd.Parameters.Add(new DB2Parameter("@email", userParam.UserName));
                                    cmd.ExecuteNonQuery();                                    
                                }
                                return true;
                            }

                        }
                    }
                }
                return false;
            }  
        }



    }
}