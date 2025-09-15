using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CadEye_WebVersion.Services.Google.Models
{
    public class LoginEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? GoogleId { get; set; }      
        public string? Email { get; set; }     
        public string? Name { get; set; }         
        public string? AccessToken { get; set; }   
        public string? RefreshToken { get; set; }  
        public DateTime LoginAt { get; set; }     
        public string? ProfilePicture { get; set; } 
    }
}
