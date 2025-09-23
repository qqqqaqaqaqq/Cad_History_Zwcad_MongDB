using CadEye_WebVersion.Models;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using MongoDB.Driver;

namespace CadEye_WebVersion.Services.Mongo.Services
{
    public class UserControlService : IUserControlService
    {
        private IMongoCollection<AdminEntity> _collection;

        public UserControlService()
        {

        }

        public void Init()
        {
            var settings = MongoClientSettings.FromConnectionString(AppSettings.ServerIP);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            var database = client.GetDatabase("CadEye_User");

            var collectionName = AppSettings.UserGoogleId;
            if (!database.ListCollectionNames().ToList().Contains(collectionName))
                database.CreateCollection(collectionName);

            _collection = database.GetCollection<AdminEntity>(collectionName);
        }

        public async Task UpdateAsync(AdminEntity file)
        {
            var filter = Builders<AdminEntity>.Filter.Eq(u => u.Googleid, file.Googleid);
            await _collection.ReplaceOneAsync(filter, file, new ReplaceOptions { IsUpsert = true });
        }

        public async Task AddAsync(AdminEntity file) =>
            await _collection.InsertOneAsync(file);

        public async Task AddOrUpdateAsync(AdminEntity file)
        {
            var filter = Builders<AdminEntity>.Filter.Eq(x => x.Googleid, file.Googleid);
            var options = new ReplaceOptions { IsUpsert = true };

            await _collection.ReplaceOneAsync(filter, file, options);

        }
        public async Task DeleteAsync(string googleid) =>
            await _collection.DeleteOneAsync(u => u.Googleid == googleid);

        public async Task<AdminEntity> FindAsync(string googleid)
        {
            var node = await _collection.Find(f => f.Googleid == googleid).FirstOrDefaultAsync();
            if (node == null) return null;
            else return node;
        }
    }
}
