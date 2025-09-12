using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CadEye_WebVersion.Services.Mongo.Services
{
    public class MongoDBEventEntry : IEventEntryService
    {
        private IMongoCollection<EventEntry> _collection;

        public MongoDBEventEntry()
        {

        }
        public void Init(string dbName)
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase(dbName);

            var collectionName = "EventEntries";
            if (!database.ListCollectionNames().ToList().Contains(collectionName))
                database.CreateCollection(collectionName);

            _collection = database.GetCollection<EventEntry>(collectionName);
        }

        public async Task AddAsync(EventEntry evt) =>
            await _collection.InsertOneAsync(evt);

        public async Task AddOrUpdateAsync(EventEntry file)
        {
            var filter = Builders<EventEntry>.Filter.Eq(x => x.Id, file.Id); 
            var options = new ReplaceOptions { IsUpsert = true };             

            await _collection.ReplaceOneAsync(filter, file, options);
        }

        public async Task<EventEntry> FindAsync(ObjectId id) =>
            await _collection.Find(f => f.Id == id).FirstOrDefaultAsync();

        public async Task<List<EventEntry>> FindAllAsync() =>
            await _collection.Find(_ => true).ToListAsync();

        public async Task DeleteAsync(ObjectId id) =>
            await _collection.DeleteOneAsync(f => f.Id == id);
    }
}
