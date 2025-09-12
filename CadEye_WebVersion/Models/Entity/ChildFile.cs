using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CadEye_WebVersion.Models.Entity
{
    public class ChildFile
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string File_FullName { get; set; } = "";
        public string File_Name { get; set; } = "";
        public string File_Directory { get; set; } = "";
        public DateTime AccesTime { get; set; } = DateTime.MinValue; // nullable 제거
        public byte[] HashToken { get; set; } = Array.Empty<byte>(); // null 안전화
    }
}
