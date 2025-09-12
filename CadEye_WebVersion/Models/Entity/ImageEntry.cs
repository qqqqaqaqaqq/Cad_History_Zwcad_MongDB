using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CadEye_WebVersion.Models.Entity
{
    public class ImageTimePath()
    {
        public DateTime? Time { get; set; }
        public string? ImagePath { get; set; }
    }

    public class ImageEntry
    {

        public ImageEntry()
        {
            Path = new List<ImageTimePath>();
        }
        [BsonId]
        public ObjectId Id { get; set; }
        public List<ImageTimePath> Path { get; set; }
    }
}
