using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Entitys
{
    public class JobListing
    {
        [BsonId] 
        [BsonRepresentation(BsonType.ObjectId)] 
        public string? Id { get; set; }

        [BsonElement("jobUrl")]
        public string JobUrl { get; set; } = string.Empty;

        [BsonElement("descriptionHash")]
        public string DescriptionHash { get; set; } = string.Empty;
       
        [BsonElement("details")]
        public Dictionary<string, string> Details { get; set; } = new Dictionary<string, string>();

        [BsonElement("extractedAt")]
        public DateTime ExtractedAt { get; set; } = DateTime.UtcNow; 
    }
}
