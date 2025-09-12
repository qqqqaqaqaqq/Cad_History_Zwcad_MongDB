using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CadEye_WebVersion.Services.Mongo.Services
{
    public class MongoDBRefEntry : IRefEntryService
    {
        private IMongoCollection<RefEntry> _collection;

        public MongoDBRefEntry()
        {

        }

        public void Init(string dbName)
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase(dbName);

            var collectionName = "RefEntries";
            if (!database.ListCollectionNames().ToList().Contains(collectionName))
                database.CreateCollection(collectionName);

            _collection = database.GetCollection<RefEntry>(collectionName);
        }

        public async Task AddAsync(RefEntry feature) =>
            await _collection.InsertOneAsync(feature);

        public async Task AddOrUpdateAsync(RefEntry file)
        {
            var filter = Builders<RefEntry>.Filter.Eq(x => x.Id, file.Id);
            var options = new ReplaceOptions { IsUpsert = true };

            await _collection.ReplaceOneAsync(filter, file, options);
        }

        public async Task<RefEntry> FindAsync(ObjectId id) =>
            await _collection.Find(f => f.Id == id).FirstOrDefaultAsync();

        public async Task<List<RefEntry>> FindAllAsync() =>
            await _collection.Find(_ => true).ToListAsync();

        public async Task DeleteAsync(ObjectId id) =>
            await _collection.DeleteOneAsync(f => f.Id == id);
    }
}
