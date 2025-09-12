using CadEye_WebVersion.Models.Entity;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace CadEye_WebVersion.Services.Mongo.Interfaces
{
    public interface IEventEntryService
    {
        void Init(string path);
        Task AddAsync(EventEntry evt);
        Task AddOrUpdateAsync(EventEntry evt);
        Task<EventEntry> FindAsync(ObjectId id);
        Task<List<EventEntry>> FindAllAsync();
        Task DeleteAsync(ObjectId id);
    }
}
