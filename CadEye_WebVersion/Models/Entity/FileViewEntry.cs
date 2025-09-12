using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CadEye_WebVersion.Models.Entity
{
    public class FileViewEntry
    {
        public FileViewEntry() { 

        }
        [BsonId]
        public ObjectId Id { get; set; }
        public required string FilePath { get; set; }
    }
}
