using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API_TEST.Models {
    public class InvoiceDetail {
        [BsonId]
        [BsonElement("_id")]
        public string? Id { get; set; }
        [BsonElement("invoice")]
        public required ObjectId InvoiceId { get; set; }
        [BsonElement("product")]
        public required ObjectId ProductId { get; set; }
        [BsonElement("quantity")]
        public required int Quantity { get; set; }
        [BsonElement("unitPrice")]
        public required decimal UnitPrice { get; set; }
        [BsonElement("total")]
        public decimal Total { get; set; }
    }
}