using API_TEST.Models;
using API_TEST.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace API_TEST.Controllers {
    [ApiController]
    [Route("api/[controller]")]

    public class ProductController : ControllerBase {
        
        private readonly IMongoCollection<Product>? _products;

        public ProductController(MongoDBService mongoDBService) {
            _products = mongoDBService.Database?.GetCollection<Product>("product");
        }

        // [HttpGet]
        // public async Task<IEnumerable<Product>> Get() {
        //     return await _products.Find(FilterDefinition<Product>.Empty).ToListAsync();
        // }

        [HttpGet("{id}")]
        public ActionResult<Product?> GetById(string id) {
            var filter = Builders<Product>.Filter.Eq(x => x.Id, id);
            var product = _products!.Find(filter).FirstOrDefault();
            return product is not null ? Ok(product) : NotFound();
        }

        // public Task<ActionResult<Product?>> GetById(string id) {
        //     var filter = Builders<Product>.Filter.Eq(x => x.Id, id);
        //     var product = _products!.Find(filter).FirstOrDefault();
        //     return Task.FromResult<ActionResult<Product?>>(product is not null ? Ok(product) : NotFound());
        // }

        // public async Task<ActionResult<Product?>> GetById(string id) {
        //     var filter = Builders<Product>.Filter.Eq(x => x.Id, id);
        //     var product = _products!.Find(filter).FirstOrDefault();
        //     return product is not null ? Ok(product) : NotFound();
        // }

        [HttpPost]
        public async Task<ActionResult> Post(Product product) {
            product.UpdatedAt = DateTime.Now;
            await _products!.InsertOneAsync(product);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut]
        public async Task<ActionResult> Update(Product product) {
            var filter = Builders<Product>.Filter.Eq(x => x.Id, product.Id);
            // var update = Builders<Product>.Update
            //     .Set(x => x.Name, product.Name)
            //     .Set(x => x.Description, product.Description)
            //     .Set(x => x.Price, product.Price)
            //     .Set(x => x.Category, product.Category)
            //     .Set(x => x.InStock, product.InStock);
            // await _products.UpdateOneAsync(filter, update);

            await _products!.ReplaceOneAsync(filter, product);
            return Ok();
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(Product product) {
            var filter = Builders<Product>.Filter.Eq(x => x.Id, product.Id);
            await _products!.DeleteOneAsync(filter);
            return Ok();
        }

        [HttpGet]
        public async Task<List<Product>> Get([FromQuery] ProductSearchParams searchParams ) {
            var filter = Builders<Product>.Filter.Empty;
            
            if (!string.IsNullOrEmpty(searchParams.Name)) {
                filter &= Builders<Product>.Filter.Eq(p => p.Name, searchParams.Name);
            }

            if (!string.IsNullOrEmpty(searchParams.Category)) {
                filter &= Builders<Product>.Filter.Eq(p => p.Category, searchParams.Category);
            }

            if (searchParams.MinPrice.HasValue) {
                filter &= Builders<Product>.Filter.Gte(p => p.Price, searchParams.MinPrice);
            }

            if (searchParams.MaxPrice.HasValue) {
                filter &= Builders<Product>.Filter.Lte(p => p.Price, searchParams.MaxPrice);
            }

            return await _products.Find(filter).ToListAsync();
        }

        // [HttpGet]
        // public async Task<List<Product>> Get([FromQuery] SearchParams searchParams) {
        //     var filter = Builders<Product>.Filter.Empty;
        //     if (!string.IsNullOrEmpty(searchParams.Id)) {
        //         filter &= Builders<Product>.Filter.Eq(p => p.Id, searchParams.Id);
        //     }

        //     if (!string.IsNullOrEmpty(searchParams.Name)) {
        //         filter &= Builders<Product>.Filter.Eq(p => p.Name, searchParams.Name);
        //     }

        //     if (!string.IsNullOrEmpty(searchParams.Category)) {
        //         filter &= Builders<Product>.Filter.Eq(p => p.Category, searchParams.Category);
        //     }

        //     return await _products.Find(filter).ToListAsync();
        // }

        [HttpPost("generateTestData")]
        public async Task GenerateTestData(int count) {
            var documents = new List<BsonDocument>();
            for (int i = 0; i < count; i++) {
                var document = new BsonDocument{
                    {"_id", null},
                    {"name", i%2 == 0 ? $"Shirt {i}" : $"Pant {i}"},
                    {"price", i},
                    {"category", i%2 == 0 ? "T-Shirt" : "Pant"},
                    {"inStock", true},
                    {"updatedAt", DateTime.Now}
                };
                documents.Add(document);
            }
            
            await _products!.InsertManyAsync((IEnumerable<Product>)documents);
        }
    }

    public class ProductSearchParams {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}