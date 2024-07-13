using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API_TEST.Models {

    public class Product {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonElement("name"), BsonRepresentation(BsonType.String)]
        public string? Name { get; set; }
        [BsonElement("description"), BsonRepresentation(BsonType.String)]
        public string? Description { get; set; }
        [BsonElement("price"), BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }
        [BsonElement("category"), BsonRepresentation(BsonType.String)]
        public string? Category { get; set; }
        [BsonElement("inStock"), BsonRepresentation(BsonType.Boolean)]
        public bool? InStock { get; set; }
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}