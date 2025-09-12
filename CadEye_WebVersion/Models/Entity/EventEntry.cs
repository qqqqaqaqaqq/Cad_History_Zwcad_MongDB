using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace CadEye_WebVersion.Models.Entity
{
    public class EventList
    {
        public DateTime Time { get; set; }
        public string? EventName { get; set; }
        public string? EventDescription { get; set; }
    }

    public class EventEntry
    {
        public EventEntry()
        {
            EventCollection = new List< EventList>();
        }

        [BsonId]
        public ObjectId Id { get; set; }
        public List<EventList> EventCollection { get; set; }
    }
}
