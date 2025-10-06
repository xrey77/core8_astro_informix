using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Google.Authenticator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using core8_astro_informix.Services;
using core8_astro_informix.Entities;
using core8_astro_informix.Models.dto;
using core8_astro_informix.Helpers;

namespace core8_astro_informix.Controllers.Products
{
    [ApiExplorerSettings(GroupName = "Get Product Id")]
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class GetProductById : ControllerBase {
        private IProductService _productService;
        private IMapper _mapper;
        private readonly IConfiguration _configuration;  
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<GetProductById> _logger;

        public GetProductById(
            IConfiguration configuration,
            IWebHostEnvironment env,
            IProductService productService,
            IMapper mapper,
            ILogger<GetProductById> logger
            )
        {
            _configuration = configuration;  
            _productService = productService;
            _mapper = mapper;
            _logger = logger;
            _env = env;        
        }  

        [HttpGet("/api/getproductid/{id}")]
        public IActionResult GetProductId(int id) {
            try {                
                var prod = _productService.GetProductById(id);
                var prods = _mapper.Map<ProductModel>(prod);
                return Ok(new {statuscode = 200, message = "Product found", product = prods});
            } catch(AppException ex) {
               return BadRequest(new {statuscode = 400, Message = ex.Message});
            }
        }
    }    
}