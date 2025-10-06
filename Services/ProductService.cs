using System.Data;
using System.IO;
using Microsoft.Extensions.Options;
using IBM.Data.Db2;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using core8_astro_informix.Entities;
using core8_astro_informix.Helpers;
using core8_astro_informix.Models;

namespace core8_astro_informix.Services
{
    public interface IProductService {
        Task<IEnumerable<Product>> ListAll(int perpage, int offset);        
        int TotPage();
        Task<IEnumerable<Product>> SearchAll(string key, int perpage, int offset);
        Task<int> TotPageSearch(string key, int perpage);
        Task<Product> CreateProduct(Product prod);
        void ProductUpdate(Product prod);
        void ProductDelete(int id);
        void UpdateProdPicture(int id, string file);
        Product GetProductById(int id);
    }

    public class ProductService : IProductService
    {
        private readonly string _connectionString;
        private readonly AppSettings _appSettings;

         IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();

        public ProductService(IOptions<AppSettings> appSettings)
        {
            _connectionString = config.GetConnectionString("InformixConnection");
            _appSettings = appSettings.Value;
        }        

        public int TotPage() {
            var perpage = 5;
            int totrecs = 0;
            string sql = "SELECT COUNT(*) FROM rey.products";
            using (var connection = new DB2Connection(_connectionString))
            {
                connection.Open();
                using (var command = new DB2Command(sql, connection))
                {
                    try
                    {
                        connection.Open();
                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            totrecs = Convert.ToInt32(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new AppException(ex.Message);
                    }
                }                

            }
            int totpage = (int)Math.Ceiling((float)(totrecs) / perpage);
            return totpage;
        }



        public async Task<IEnumerable<Product>> ListAll(int perpage, int offset)
        {
            var products = new List<Product>();
            string sql = $"SELECT id,category,descriptions,unit,qty,costprice,sellprice,saleprice,productpicture,alertstocks,criticalstocks FROM rey.products";
            await using (var connection = new DB2Connection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new DB2Command(sql, connection))
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Category = reader.GetString(reader.GetOrdinal("category")),
                                Descriptions = reader.GetString(reader.GetOrdinal("descriptions")),
                                Unit = reader.GetString(reader.GetOrdinal("unit")),
                                Qty = reader.GetInt32(reader.GetOrdinal("qty")),
                                CostPrice = reader.GetDecimal(reader.GetOrdinal("costprice")),
                                SellPrice = reader.GetDecimal(reader.GetOrdinal("sellprice")),
                                SalePrice = reader.GetDecimal(reader.GetOrdinal("saleprice")),
                                ProductPicture = reader.GetString(reader.GetOrdinal("productpicture")),
                                AlertStocks = reader.GetInt32(reader.GetOrdinal("alertstocks")),
                                CriticalStocks = reader.GetInt32(reader.GetOrdinal("criticalstocks"))
                            });
                        }
                    }
                }                                
            }
            return products;            
            // var products1 = await _context.Products
            // .OrderBy(p => p.Id)
            // .Where(e => e.Id > offset) 
            // .Take(perpage)
            // .ToListAsync();
            // return products1;
        }

        public async Task<int> TotPageSearch(string key, int perpage) {
            string sql = "SELECT COUNT(*) FROM rey.products WHERE descriptions LIKE @key";
            int totalRecords = 0;
            await using (var connection = new DB2Connection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new DB2Command(sql, connection))
                {
                    command.Parameters.Add(new DB2Parameter("@key", key));
                    try
                    {
                        await connection.OpenAsync();
                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            totalRecords = Convert.ToInt32(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new AppException(ex.Message);
                    }
                }                

            }            
            int totpage = (int)Math.Ceiling((float)(totalRecords) / perpage);
            return totpage;
        }


        public async Task<IEnumerable<Product>> SearchAll(string key, int perpage, int offset)
        {
            var products = new List<Product>();
            string sql = $"SELECT id,category,descriptions,unit,qty,costprice,sellprice,saleprice,productpicture,alertstocks,criticalstocks FROM rey.products WHERE descriptions LIKE @key";
            await using (var connection = new DB2Connection(_connectionString))
            {
                await connection.OpenAsync();
                await using (var command = new DB2Command(sql, connection))
                {
                    command.Parameters.Add(new DB2Parameter("@key", key));
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(new Product
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Category = reader.GetString(reader.GetOrdinal("category")),
                                Descriptions = reader.GetString(reader.GetOrdinal("descriptions")),
                                Unit = reader.GetString(reader.GetOrdinal("unit")),
                                Qty = reader.GetInt32(reader.GetOrdinal("qty")),
                                CostPrice = reader.GetDecimal(reader.GetOrdinal("costprice")),
                                SellPrice = reader.GetDecimal(reader.GetOrdinal("sellprice")),
                                SalePrice = reader.GetDecimal(reader.GetOrdinal("saleprice")),
                                ProductPicture = reader.GetString(reader.GetOrdinal("productpicture")),
                                AlertStocks = reader.GetInt32(reader.GetOrdinal("alertstocks")),
                                CriticalStocks = reader.GetInt32(reader.GetOrdinal("criticalstocks"))
                            });
                        }
                    }
                }                                
            }
            return products;                    
            // var products3 = await _context.Products.FromSql($"SELECT * FROM PRODUCTS WHERE LOWER(DESCRIPTIONS) LIKE {key} ORDER BY ID").ToListAsync(); 
            // var records = products3.OrderBy(p => p.Id).Where(e => e.Id > offset).Take(perpage).ToList();
            // return records;
        }

        public async Task<Product> CreateProduct(Product prod) {
            string sqlInsert = "INSERT INTO rey.products(category,descriptions,unit,qty,costprice,sellprice,saleprice,productpicture,alertstocks,criticalstocks) VALUES(@fld1,@fld2,@fld3,@fld4,@fld4,@fld5,@fld6,@fld7,@fld8,@fld9,@fld10)";
            string sqlFind = "SELECT * FROM rey.products WHERE descriptions = @desc FETCH FIRST 1 ROW ONLY";
            await using (var connection = new DB2Connection(_connectionString))
            {
              await connection.OpenAsync();
              try {

                using (var command = new DB2Command(sqlFind, connection))
                {
                    command.Parameters.Add(new DB2Parameter("@desc", prod.Descriptions));
                    using (var reader = command.ExecuteReader())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (reader.GetString(reader.GetOrdinal("descriptions")) is not null) {
                                throw new AppException("Product Description is already exists...");
                            }
                        }
                    }

                    await using (var cmd = new DB2Command(sqlInsert, connection))
                    {
                        cmd.Parameters.Add(new DB2Parameter("@fld1", prod.Category));
                        cmd.Parameters.Add(new DB2Parameter("@fld2", prod.Descriptions));
                        cmd.Parameters.Add(new DB2Parameter("@fld3", prod.Unit));
                        cmd.Parameters.Add(new DB2Parameter("@fld4", prod.Qty));
                        cmd.Parameters.Add(new DB2Parameter("@fld5", prod.CostPrice));
                        cmd.Parameters.Add(new DB2Parameter("@fld6", prod.SellPrice));
                        cmd.Parameters.Add(new DB2Parameter("@fld7", prod.SalePrice));
                        cmd.Parameters.Add(new DB2Parameter("@fld8", prod.ProductPicture));
                        cmd.Parameters.Add(new DB2Parameter("@fld9", prod.AlertStocks));
                        cmd.Parameters.Add(new DB2Parameter("@fld10", prod.CriticalStocks));
                        cmd.ExecuteNonQuery();
                    }
                 }
                 return prod;
              } catch(Exception) {
                throw new AppException("Product not found");
              }                
            }      
        }

        public void ProductUpdate(Product prod) {
            DateTime now = DateTime.Now;
            var sql = "UPDATE rey.products SET category=@category,descriptions=@descriptions,unit=@unit,qty=@qty,costprice=@costprice,sellprice=@sellprice,saleprice=@salesprice,productpicture=@productpicture,alertstocks=@alertstocks,criticalstocks=@criticalstocks, updateat = @update WHERE id = @id";
            using (var connection = new DB2Connection(_connectionString))
            {
                connection.Open();
                try {
                    using (var cmd = new DB2Command(sql, connection))
                    {
                        cmd.Parameters.Add(new DB2Parameter("@category", prod.Category));
                        cmd.Parameters.Add(new DB2Parameter("@descriptions", prod.Descriptions));
                        cmd.Parameters.Add(new DB2Parameter("@unit", prod.Unit));
                        cmd.Parameters.Add(new DB2Parameter("@qty", prod.Qty));
                        cmd.Parameters.Add(new DB2Parameter("@costprice", prod.CostPrice));
                        cmd.Parameters.Add(new DB2Parameter("@sellprice", prod.SellPrice));
                        cmd.Parameters.Add(new DB2Parameter("@saleprice", prod.SalePrice));
                        cmd.Parameters.Add(new DB2Parameter("@productpicture", prod.ProductPicture));
                        cmd.Parameters.Add(new DB2Parameter("@alertstocks", prod.AlertStocks));
                        cmd.Parameters.Add(new DB2Parameter("@criticalstocks", prod.CriticalStocks));
                        cmd.Parameters.Add(new DB2Parameter("@update", now));
                        cmd.Parameters.Add(new DB2Parameter("@id", prod.Id));
                        cmd.ExecuteNonQuery();
                    }
                } catch(Exception) {
                    throw new AppException("Product not found");
                }                
            }                
        }

        public void ProductDelete(int id) {
            string sql = "DELETE FROM rey.products WHERE id = @id";
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
                    throw new AppException("Product not found");
                }
            }                     
        }

        public void UpdateProdPicture(int id, string file) {
            DateTime now = DateTime.Now;
            var sql = "UPDATE rey.products SET productpicture = @productpicture, updateat = @update WHERE id = @id";
            using (var connection = new DB2Connection(_connectionString))
            {
                connection.Open();
                try {
                    using (var cmd = new DB2Command(sql, connection))
                    {
                        cmd.Parameters.Add(new DB2Parameter("@productpicture", file));
                        cmd.Parameters.Add(new DB2Parameter("@update", now));
                        cmd.Parameters.Add(new DB2Parameter("@id", id));
                        cmd.ExecuteNonQuery();
                    }
                } catch(Exception) {
                    throw new AppException("Product not found");
                }                
            }                                   
        }

        public Product GetProductById(int id) {
            string sql = "SELECT id,category,descriptions,unit,qty,costprice,sellprice,saleprice,productpicture,alertstocks,criticalstocks FROM rey.products WHERE id = @id";
            var product = new Product();
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
                            product.Id = reader.GetInt32(reader.GetOrdinal("id"));
                            product.Category = reader.GetString(reader.GetOrdinal("category"));
                            product.Descriptions = reader.GetString(reader.GetOrdinal("descriptions"));
                            product.Unit = reader.GetString(reader.GetOrdinal("unit"));
                            product.Qty = reader.GetInt32(reader.GetOrdinal("qty"));
                            product.CostPrice = reader.GetDecimal(reader.GetOrdinal("costprice"));
                            product.SellPrice = reader.GetDecimal(reader.GetOrdinal("sellprice"));
                            product.SalePrice = reader.GetDecimal(reader.GetOrdinal("saleprice"));
                            product.ProductPicture = reader.GetString(reader.GetOrdinal("productpicture"));
                            product.AlertStocks = reader.GetInt32(reader.GetOrdinal("alertstocks"));
                            product.CriticalStocks = reader.GetInt32(reader.GetOrdinal("criticalstocks"));

                        }
                    }
                 }
                 return product;
                } catch(Exception) {
                    throw new AppException("Product not found");
                }
            }            
        }        


    }
}