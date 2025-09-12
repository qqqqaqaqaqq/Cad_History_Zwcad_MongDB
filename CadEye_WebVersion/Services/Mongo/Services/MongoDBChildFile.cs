using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CadEye_WebVersion.Services.Mongo.Services
{
    public class MongoDBChildFile : IChildFileService
    {
        private IMongoCollection<ChildFile> _collection;

        public MongoDBChildFile()
        {

        }

        public void Init(string dbName)
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase(dbName);

            var collectionName = "ChildFiles";
            if (!database.ListCollectionNames().ToList().Contains(collectionName))
                database.CreateCollection(collectionName);

            _collection = database.GetCollection<ChildFile>(collectionName);
        }

        public async Task AddAsync(ChildFile file) =>
            await _collection.InsertOneAsync(file);

        public async Task AddOrUpdateAsync(ChildFile file)
        {
            var filter = Builders<ChildFile>.Filter.Eq(x => x.Id, file.Id); 
            var options = new ReplaceOptions { IsUpsert = true };  

            await _collection.ReplaceOneAsync(filter, file, options);
        }

        public async Task AddAllAsync(List<ChildFile> files) =>
            await _collection.InsertManyAsync(files);

        public async Task<ChildFile> FindAsync(ObjectId id) =>
            await _collection.Find(f => f.Id == id).FirstOrDefaultAsync();

        public async Task<ChildFile> NameFindAsync(string path) =>
            await _collection.Find(f => f.File_FullName == path).FirstOrDefaultAsync();

        public async Task<List<ChildFile>> FindAllAsync() =>
            await _collection.Find(_ => true).ToListAsync();
            

        public async Task UpdateAsync(ObjectId id, string newFeaturePath)
        {
            var update = Builders<ChildFile>.Update.Set(f => f.File_FullName, newFeaturePath);
            await _collection.UpdateOneAsync(f => f.Id == id, update);
        }

        public async Task DeleteAsync(ObjectId id) =>
            await _collection.DeleteOneAsync(f => f.Id == id);
    }
}
