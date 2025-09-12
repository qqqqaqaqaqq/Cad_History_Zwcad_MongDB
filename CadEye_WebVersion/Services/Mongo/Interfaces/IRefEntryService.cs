using CadEye_WebVersion.Models.Entity;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CadEye_WebVersion.Services.Mongo.Interfaces
{
    public interface IRefEntryService
    {
        void Init(string path);
        Task AddAsync(RefEntry feature);
        Task AddOrUpdateAsync(RefEntry feature);
        Task<RefEntry> FindAsync(ObjectId id);
        Task<List<RefEntry>> FindAllAsync();
        Task DeleteAsync(ObjectId id);
    }
}
