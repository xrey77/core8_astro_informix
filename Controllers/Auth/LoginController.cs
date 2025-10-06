using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System;
using core8_astro_informix.Services;
using core8_astro_informix.Models.dto;
using core8_astro_informix.Helpers;
using core8_astro_informix.Entities;

namespace core8_astro_informix.Controllers.Auth
{
    
[ApiExplorerSettings(GroupName = "Sign-in to User Account")]
[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    private readonly IJWTTokenServices _jwttokenservice;

    private IAuthService _authService;
    private IMapper _mapper;
    private readonly IConfiguration _configuration;  

    private readonly IWebHostEnvironment _env;

    private readonly ILogger<LoginController> _logger;

    public LoginController(
        IJWTTokenServices jWTTokenServices,
        IConfiguration configuration,
        IWebHostEnvironment env,
        IAuthService authService,
        IMapper mapper,
        ILogger<LoginController> logger
        )
    {
        _jwttokenservice = jWTTokenServices;        
        _configuration = configuration;  
        _authService = authService;
        _mapper = mapper;
        _logger = logger;
        _env = env;        
    }  

    [HttpPost("/signin")]
    public async Task<IActionResult> signin([FromBody]UserLogin model) {
            try {
                    var xuser = await _authService.SigninUser(model.Username, model.Password_hash);
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, xuser.Id.ToString()),
                            new Claim(ClaimTypes.Name, xuser.UserName)
                            // Add other claims as needed
                        }),
                        Expires = DateTime.UtcNow.AddHours(8), // Set token expiration
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                        Issuer = _configuration["Jwt:Issuer"],
                        Audience = _configuration["Jwt:Audience"]
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var tokenString = tokenHandler.WriteToken(token);
                    return Ok(new { 
                        statuscode = 200,
                        message = "Login Successfull..",
                        id = xuser.Id,
                        lastname = xuser.LastName,
                        firstname = xuser.FirstName,
                        username = xuser.UserName,
                        roles = xuser.Roles,
                        IsActivated = xuser.IsActivated,
                        isblocked = xuser.Isblocked,
                        profilepic = xuser.Profilepic,
                        qrcodeurl = xuser.Qrcodeurl,
                        token = tokenString
                        });
            }
            catch (AppException ex)
            {
                return BadRequest(new {statuscode = 400, Message = ex.Message});
            }

    }
}

    
}