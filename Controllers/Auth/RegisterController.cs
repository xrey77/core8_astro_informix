using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.IO;
using core8_astro_informix.Models.dto;
using core8_astro_informix.Services;
using core8_astro_informix.Entities;
using core8_astro_informix.Helpers;

namespace core8_astro_informix.Controllers.Auth
{
    
[ApiExplorerSettings(GroupName = "Sign-up or Account Registration")]
[ApiController]
[Route("[controller]")]
public class RegisterController : ControllerBase
{
    public IAuthService _authService;
    private readonly ILogger<RegisterController> _logger;    
    // private EmailService _emailService;    
    private IMapper _mapper;
    // private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;  
    
    public RegisterController(
        IWebHostEnvironment env,
        IAuthService userService,
        // EmailService emailService,
        IConfiguration configuration,
        IMapper mapper,
        ILogger<RegisterController> logger
        )
    {   
        _authService = userService;
        // _emailService = emailService;
        _configuration = configuration;  
        _mapper = mapper;
        _logger = logger;
        // _env = env;
    }  



    [HttpPost("/signup")]
    public async Task<IActionResult> signup([FromBody]UserRegister model) {


        DateTime now = DateTime.Now;
        var user = _mapper.Map<User>(model);
            try
            {   
                user.LastName = model.LastName;
                user.FirstName = model.FirstName;
                user.Email = model.Email;
                user.Mobile = model.Mobile;
                user.UserName = model.Username;            
                user.IsActivated = 1;
                user.Isblocked = 0;
                user.Mailtoken = 0;
                user.Profilepic = "https://localhost:7289/users/pix.png";
                user.Roles = "USER";
                // user.CreatedAt = now;
                user.UpdatedAt = now;

                var data = await _authService.SignupUser(user, model.Password_hash);
        //         // string fullname = model.Firstname + " " + model.Lastname;
        //         // string emailaddress = model.Email;
        //         // string htmlmsg = "<div><p>Please click Activate button below to confirm you email address and activate your account.</p>"+
        //         //             "<a href=\"https://localhost:7280/api/activateuser/id=" + user.Id.ToString() + "\" style=\"background-color: green;color:white;text-decoration: none;border-radius: 20px; \">&nbsp;&nbsp; Activate Account &nbsp;&nbsp;</a></div>";
        //         // string subject = "Barclays Account Activation";                
        //         // IF YOU WISH TO USE USER EMAIL ACTIVATION, JUST UNCOMMENT _emailService.sendMail
        //         // _emailService.sendMail(emailaddress, fullname, subject, htmlmsg);
        //         // and comment  user.Isactivated = 1;    
        //         // _authService.CreatedAtAction(nameof(User), new { id = user.Id }, user);

                return Ok(new {statuscode = 200, message = "Registration successfull, please login now."});
            }
            catch (AppException ex)
            {
                _logger.LogError(ex, "Error connecting to Informix.");
                return BadRequest(new { statuscode = 400, message = ex.Message });
            }
    }
 }    
}