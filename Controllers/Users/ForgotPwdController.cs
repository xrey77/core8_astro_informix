using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.IO;
using AutoMapper;
using core8_astro_informix.Entities;
using core8_astro_informix.Services;
using core8_astro_informix.Models.dto;
using core8_astro_informix.Helpers;

namespace core8_astro_informix.Controllers.Users
{
    [ApiExplorerSettings(GroupName = "Forgot User Password")]
    [ApiController]
    [Route("[controller]")]
    public class ForgotPwdController : ControllerBase {

    private IMapper _mapper;
    private IUserService _userService;
    private EmailService _emailService;    
    private readonly IConfiguration _configuration;  
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ForgotPwdController> _logger;

    public ForgotPwdController(
        IConfiguration configuration,
        IWebHostEnvironment env,
        IMapper mapper,
        IUserService userService,
        EmailService emailService,
        ILogger<ForgotPwdController> logger
        )
    {
        _configuration = configuration;  
        _logger = logger;
        _mapper = mapper;
        _userService = userService;
        _emailService = emailService;
        _env = env;        
    }  

        //Forgot Password
        [HttpPatch("/api/resetpassword/{username}")]
        public async Task<IActionResult> ResetPassword(string username, [FromBody]ForgotPassword model)
        {
            try {                
                var user = new User {
                    UserName = username,
                    Password_hash = model.Password_hash,
                    Mailtoken = model.Mailtoken
                };
                await _userService.ChangePassword(user);
                return Ok(new {statuscode = 200, message = "Password successfully changed.." });
            } catch (AppException ex) {
                return BadRequest(new { statuscode = 400, message = ex.Message });
            }
        }

        [HttpPost("/api/emailtoken")]
        public async Task<IActionResult> EmailToken([FromBody]MailTokenModel model)
        {
           try {
             int etoken = await _userService.SendEmailToken(model.Email);   
                //SET UP EMAIL SERVICE, change EmailSettings configuration in appsettings.json                       
                try {
                     _emailService.sendMailToken(model.Email,"Mail Token","Please copy or enter this token in forgot password option. " + etoken.ToString());
                } catch(Exception ex) {
                    return BadRequest(new { statuscode = 400, message = ex.Message });
                }
                return Ok(new {statuscode = 200, message= "Your Mailtoken has been sent to your Email Address", etoken = etoken});
           }
            catch (AppException ex)
            {
                return BadRequest(new { statuscode = 400, message = ex.Message });
            }

        }


    


    }    
}