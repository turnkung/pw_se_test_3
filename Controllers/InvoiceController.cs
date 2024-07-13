using API_TEST.Models;
using API_TEST.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace API_TEST.Controllers {
    [ApiController]
    [Route("api/[controller]")]

    public class InvoiceController : ControllerBase {
        private readonly IMongoCollection<Invoice>? _invoices;
        private readonly IMongoCollection<InvoiceDetail>? _invoiceDetails;

        public InvoiceController(MongoDBService mongoDBService) {
            _invoices = mongoDBService.Database?.GetCollection<Invoice>("invoice");
            _invoiceDetails = mongoDBService.Database?.GetCollection<InvoiceDetail>("invoiceDetail");
        }
        
        [HttpGet]
        public async Task<List<Invoice>> Get([FromQuery] InvoiceSearchParams searchParams) {
            var filter = Builders<Invoice>.Filter.Empty;

            if (!string.IsNullOrEmpty(searchParams.Id)) {
                filter &= Builders<Invoice>.Filter.Eq(i => i.Id, searchParams.Id);
            }
            // if (!string.IsNullOrEmpty(searchParams.Customer)) {
            //     filter &= Builders<Invoice>.Filter.Eq(i => i.Customer, searchParams.Customer);
            // }
            if (!string.IsNullOrEmpty(searchParams.Status)) {
                filter &= Builders<Invoice>.Filter.Eq(i => i.Status, searchParams.Status);
            }

            return await _invoices.Find(filter).ToListAsync();
        }

        [HttpGet("{id}")]
        public ActionResult<Invoice?> GetById(string id) {
            var filter = Builders<Invoice>.Filter.Eq(x => x.Id, id);
            var invoice = _invoices.Find(filter).FirstOrDefault();
            return invoice is not null ? Ok(invoice) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult> Post(Invoice invoice) {
            await _invoices!.InsertOneAsync(invoice);
            return CreatedAtAction(nameof(GetById), new {id = invoice.Id});
        }

        [HttpPost("generateTestData")]
        public async Task GenerateTestData(int count) {
            var documents = new List<BsonDocument>();
            for (int i = 0; i< count; i++) {
                var document = new BsonDocument{
                    {"_id", null},
                    {"invoiceDate", DateTime.Now},
                    {"totalAmount", 0},
                    {"staus", "Unpaid"}
                };
                documents.Add(document);
            }
            await _invoices!.InsertManyAsync((IEnumerable<Invoice>) documents);
        }
    }

    public class InvoiceSearchParams {
        public string? Id { get; set; }
        // public string? Customer { get; set; }
        // public DateTime? InvoiceDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Status { get; set; }
    }
}