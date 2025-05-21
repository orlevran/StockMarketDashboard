using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace UsersMicroservice.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime lastLogin { get; set; } = DateTime.UtcNow;
        public DateTime lastUpdate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
