using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API_TEST.Models {

    public class Invoice {
        [BsonId]
        [BsonElement("_id")]
        public string? Id { get; set; }
        // [BsonElement("customer"), ForeignKey("customer")]
        // public required string Customer { get; set; }
        [BsonElement("invoiceDate")]
        public DateTime InvoiceDate { get; set; }
        [BsonElement("totalAmount")]
        public decimal TotalAmount { get; set; }
        [BsonElement("status")]
        public required string Status { get; set; }
    }
}