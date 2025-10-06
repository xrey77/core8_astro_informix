using core8_astro_informix.Services;
using core8_astro_informix.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.IO;

namespace core8_astro_informix.Controllers.Users
{
    [ApiExplorerSettings(GroupName = "Forgot User Password")]
    [ApiController]
    [Route("[controller]")]
    public class ActivateUserController : ControllerBase {
    private IUserService _userService;
    private EmailService _emailService;    
    private readonly IConfiguration _configuration;  
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ActivateUserController> _logger;

    public ActivateUserController(
        IConfiguration configuration,
        IWebHostEnvironment env,
        EmailService emailService,
        IUserService userService,
        ILogger<ActivateUserController> logger
        )
    {
        _configuration = configuration;  
        _emailService = emailService;
        _userService = userService;
        _logger = logger;
        _env = env;        
    }  

        [HttpGet("/api/activateuser/{id}")]
        public IActionResult ActivateUser(int id) {
            try
            {
                    //GET USER INFO
                    var user = _userService.GetById(id);
                    string email = user.Email;
                    string fullname = user.FirstName + " " + user.LastName;
                    string subj = "Account Activation Confirmation";
                    string htmlmsg = "<div><p><strong>Congratiolation</strong>, your Account has been activated successfully..</p></div>";
                    _userService.ActivateUser(id);
                    //SEND ACTIONVATION CONFIRMATION
                  _emailService.sendMail(email, fullname, subj, htmlmsg);
                return Ok(new {statuscode = 200, message = "Your Account is activated successfully."});
            }
            catch (AppException ex)
            {
                return BadRequest(new { statuscode = 400, message = ex.Message });
            }
        }


    }    
}