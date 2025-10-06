using System.IO;
using System;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using core8_astro_informix.Services;
using core8_astro_informix.Entities;
using core8_astro_informix.Models.dto;
using core8_astro_informix.Helpers;

namespace core8_astro_informix.Controllers.Products
{
    [ApiExplorerSettings(GroupName = "Upload Product Picture")]
    [ApiController]
    [Route("[controller]")]
    public class UploadProductPicture : ControllerBase {
        private IProductService _productService;
        private IMapper _mapper;
        private readonly IConfiguration _configuration;  
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<UploadProductPicture> _logger;

        public UploadProductPicture(
            IConfiguration configuration,
            IWebHostEnvironment env,
            IProductService productService,
            IMapper mapper,
            ILogger<UploadProductPicture> logger
            )
        {
            _configuration = configuration;  
            _productService = productService;
            _mapper = mapper;
            _logger = logger;
            _env = env;        
        }  

        [HttpPatch("/api/uploadproductpicture/{id}")]
        public IActionResult ProdupdatePicture(UploadProductpicModel model) {
                if (model.ProductPicture.FileName != null)
                {
                    try
                    {
                        string ext= Path.GetExtension(model.ProductPicture.FileName);
                        var folderName = Path.Combine("wwwroot", "products/");
                        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                        var newFilename =pathToSave + "00" + model.Id + ".jpg";

                        using var image = SixLabors.ImageSharp.Image.Load(model.ProductPicture.OpenReadStream());
                        image.Mutate(x => x.Resize(100, 100));
                        image.Save(newFilename);
                        string file = "https://localhost:7289/products/00"+model.Id.ToString()+".jpg";
                        if (model.ProductPicture != null)
                        {
                            _productService.UpdateProdPicture(model.Id, file);                            
                        }
                        return Ok(new { 
                            statuscode = 200, 
                            message = "Product Picture has been updated.",
                            productpic = file});                        
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new {statuscode = 400, message =ex.Message});
                    }
                }
                return BadRequest(new { statuscode = 400, message = "Profile Picture not found."});
        }
    }    
}