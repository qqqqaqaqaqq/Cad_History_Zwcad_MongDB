using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CadEye_WebVersion.Models.Entity
{
    public class ProjectPath
    {

        [BsonId]
        public ObjectId Id { get; set; }
        public string ProjectFullName { get; set; } = string.Empty;
    }
}
