using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CadEye_WebVersion.Services.Mongo.Services
{
    public class MongoDBProjectPath : IProjectPath
    {
        private IMongoCollection<ProjectPath> _collection;

        public MongoDBProjectPath()
        {

        }

        public void Init(string dbName)
        {
            var settings = MongoClientSettings.FromConnectionString(AppSettings.ServerIP);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            var database = client.GetDatabase(dbName);

            var collectionName = "ProjectPaths";
            if (!database.ListCollectionNames().ToList().Contains(collectionName))
                database.CreateCollection(collectionName);

            _collection = database.GetCollection<ProjectPath>(collectionName);
        }

        public async Task AddAsync(ProjectPath project) =>
            await _collection.InsertOneAsync(project);

        public async Task<ProjectPath> NameFindAsync(string projectname)
        {
            var node = await _collection.Find(f => f.ProjectFullName.Contains(projectname)).FirstOrDefaultAsync();
            if (node == null)
                return null;
            else
                return node;
        }

        public async Task DeleteAsync(ObjectId id) =>
            await _collection.DeleteOneAsync(f => f.Id == id);
    }
}
