using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace CadEye_WebVersion.Models.Entity
{
    public class RefsList
    {
        public DateTime? Time {  get; set; }
        public List<string>? Ref { get; set; }
    }

    public class RefEntry
    {
        public RefEntry()
        {
            Refs = new List<RefsList>();
        }

        [BsonId]
        public ObjectId Id { get; set; }
        public List<RefsList> Refs { get; set; }
    }
}
