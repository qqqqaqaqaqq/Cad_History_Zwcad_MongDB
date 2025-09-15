using CadEye_WebVersion.Services.Google.Models;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using MongoDB.Driver;

namespace CadEye_WebVersion.Services.Mongo.Services
{
    public class UserControlService : IUserControlService
    {
        private IMongoCollection<LoginEntity> _collection;

        public UserControlService()
        {

        }

        public void Init(string dbName)
        {
            var settings = MongoClientSettings.FromConnectionString(AppSettings.ServerIP);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            var database = client.GetDatabase(dbName);

            var collectionName = "Users";
            if (!database.ListCollectionNames().ToList().Contains(collectionName))
                database.CreateCollection(collectionName);

            _collection = database.GetCollection<LoginEntity>(collectionName);
        }

        public async Task AddAsync(LoginEntity file) =>
            await _collection.InsertOneAsync(file);

        public async Task DeleteAsync(string googleId) =>
            await _collection.DeleteOneAsync(u => u.GoogleId == googleId);

        public async Task<LoginEntity> FindAsync(string id) =>
            await _collection.Find(f => f.GoogleId == id).FirstOrDefaultAsync();
    }
}
