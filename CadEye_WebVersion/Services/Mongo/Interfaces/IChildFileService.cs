using CadEye_WebVersion.Models.Entity;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CadEye_WebVersion.Services.Mongo.Interfaces
{
    public interface IChildFileService
    {
        void Init(string path);
        Task AddAsync(ChildFile file);
        Task AddOrUpdateAsync(ChildFile file);
        Task AddAllAsync(List<ChildFile> files);
        Task<ChildFile> FindAsync(ObjectId id);
        Task<ChildFile> NameFindAsync(string path);
        Task<List<ChildFile>> FindAllAsync();
        Task DeleteAsync(ObjectId id);
    }
}
