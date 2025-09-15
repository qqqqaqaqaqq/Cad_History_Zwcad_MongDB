using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CadEye_WebVersion.Services.Mongo.Services
{
    public class MongoDBImageEntry : IImageEntryService
    {
        private IMongoCollection<ImageEntry> _collection;

        public MongoDBImageEntry()
        {

        }

        public void Init(string dbName)
        {
            var settings = MongoClientSettings.FromConnectionString(AppSettings.ServerIP);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            var database = client.GetDatabase(dbName);

            var collectionName = "ImageEntries";
            if (!database.ListCollectionNames().ToList().Contains(collectionName))
                database.CreateCollection(collectionName);

            _collection = database.GetCollection<ImageEntry>(collectionName);
        }

        public async Task AddAsync(ImageEntry image) =>
            await _collection.InsertOneAsync(image);

        public async Task AddOrUpdateAsync(ImageEntry file)
        {
            var filter = Builders<ImageEntry>.Filter.Eq(x => x.Id, file.Id);
            var options = new ReplaceOptions { IsUpsert = true };

            await _collection.ReplaceOneAsync(filter, file, options);
        }

        public async Task<ImageEntry> FindAsync(ObjectId id) =>
            await _collection.Find(f => f.Id == id).FirstOrDefaultAsync();

        public async Task<List<ImageEntry>> FindAllAsync() =>
            await _collection.Find(_ => true).ToListAsync();

        public async Task DeleteAsync(ObjectId id) =>
            await _collection.DeleteOneAsync(f => f.Id == id);
    }
}
