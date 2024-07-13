using API_TEST.Models;
using API_TEST.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace API_TEST.Controllers {
    [ApiController]
    [Route("api/[controller]")]

    public class InvoiceDetailController : ControllerBase {
        private readonly IMongoCollection<Invoice>? _invoices;
        private readonly IMongoCollection<Product>? _products;
        private readonly IMongoCollection<InvoiceDetail>? _invoiceDetails;

        public InvoiceDetailController(MongoDBService mongoDBService) {
            _invoiceDetails = mongoDBService.Database?.GetCollection<InvoiceDetail>("invoiceDetail");
            _invoices = mongoDBService.Database?.GetCollection<Invoice>("invoice");
            _products = mongoDBService.Database?.GetCollection<Product>("product");
        }

        [HttpGet("{id}")]
        public ActionResult<InvoiceDetail?> GetById(string id) {
            var filter = Builders<InvoiceDetail>.Filter.Eq(i => i.Id, id);
            var invoiceDetail = _invoiceDetails.Find(filter).FirstOrDefault();
            return invoiceDetail is not null ? Ok(invoiceDetail) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult> Post(InvoiceDetail invoiceDetail) {
            await _invoiceDetails!.InsertOneAsync(invoiceDetail);
            return CreatedAtAction(nameof(GetById), new {id = invoiceDetail.Id}, invoiceDetail);
        }

        [HttpPost("generateTestData")]
        public async Task GenerateTestData(int count) {
            var invoiceFilter = Builders<Invoice>.Filter.Empty;
            var invoiceProjection = Builders<Invoice>.Projection.Include("_id");
            var invoiceDocuments = await _invoices.Find(invoiceFilter).Project(invoiceProjection).ToListAsync();

            var invoiceIdList = new List<ObjectId>();
            foreach (var doc in invoiceDocuments) {
                invoiceIdList.Add(doc["_id"].AsObjectId);
            }

            var productFilter = Builders<Product>.Filter.Empty;
            var productDocuments = await _products.Find(productFilter).ToListAsync();

            var invoiceToUpdate = new List<ObjectId>();

            var documents = new List<BsonDocument>();
            for (int i = 0; i < count; i++) {
                Random r = new Random();
                int rInt = r.Next(0, productDocuments.Count);
                int invoiceIdx = r.Next(0, invoiceDocuments.Count);
                // int index = documents.FindIndex(x => x["product"] == productDocuments[rInt].Id);
                
                // Trigger when found duplication product.
                /* if (index != -1) {
                    BsonDocument newDoc = new BsonDocument{

                    };
                } */

                var document = new BsonDocument{
                    {"_id", null},
                    {"invoice", invoiceIdList[invoiceIdx]},
                    {"product", productDocuments[rInt].Id},
                    {"quantity", rInt},
                    {"unitPrice", productDocuments[rInt].Price},
                    {"total", rInt * productDocuments[rInt].Price}
                };

                documents.Add(document);
                invoiceToUpdate.Add(invoiceIdList[invoiceIdx]);
            }

            await _invoiceDetails!.InsertManyAsync((IEnumerable<InvoiceDetail>) documents);
            await UpdateInvoiceTotal(invoiceToUpdate);
        }

        public async Task UpdateInvoiceTotal(List<ObjectId> invoiceIds) {
            foreach (var invoiceId in invoiceIds) {
                var invoiceDetailFilter = Builders<InvoiceDetail>.Filter.Eq(i => i.InvoiceId, invoiceId);
                var invoiceProjection = Builders<InvoiceDetail>.Projection.Expression(d => d.Total);
                var total = await _invoiceDetails.Find(invoiceDetailFilter).Project(invoiceProjection).ToListAsync();
                var invoiceTotal = total.Sum();

                Random ran = new Random();
                var invoiceFilter = Builders<Invoice>.Filter.Eq("_id", invoiceId);
                var update = Builders<Invoice>.Update.Set("total", invoiceTotal).Set("status", ran.Next(0, 10) % 2 == 0 ? "Paid" : "Unpaid");
                await _invoices!.UpdateOneAsync(invoiceFilter, update);
            }
        }
    }
}