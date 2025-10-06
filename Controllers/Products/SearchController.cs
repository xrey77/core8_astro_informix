using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using core8_astro_informix.Services;
using core8_astro_informix.Models.dto;
using core8_astro_informix.Helpers;
using core8_astro_informix.Models;

namespace core8_astro_informix.Controllers.Products
{
    [ApiExplorerSettings(GroupName = "Search Product Description")]
    [ApiController]
    [Route("[controller]")]
    public class SearchController : ControllerBase {

        private IProductService _productService;

        private IMapper _mapper;
        private readonly IConfiguration _configuration;  

        private readonly IWebHostEnvironment _env;

        private readonly ILogger<SearchController> _logger;

        public SearchController(
            IConfiguration configuration,
            IWebHostEnvironment env,
            IProductService productService,
            IMapper mapper,
            ILogger<SearchController> logger
            )
        {
            _configuration = configuration;  
            _productService = productService;
            _mapper = mapper;
            _logger = logger;
            _env = env;        
        }  

        [HttpGet("/api/searchproducts/{page}/{key}")]
        public IActionResult SearchProducts(int page, string key) {
            try {                
                var searchKey = "%" + key + "%";
                int perpage = 5;
                int offset = (page -1) * perpage;
                var totalpage = _productService.TotPageSearch(searchKey, perpage);
                var products = _productService.SearchAll(searchKey, perpage, offset);
                var model = _mapper.Map<IList<ProductModel>>(products);
                return Ok(new {totpage = totalpage, page = page, products=model});
            } catch(AppException ex) {
               return BadRequest(new {statuscode = 400, message = ex.Message});
            }
        }
    }    
}