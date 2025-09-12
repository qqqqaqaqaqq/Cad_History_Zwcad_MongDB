using CadEye_WebVersion.Models.Entity;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CadEye_WebVersion.Services.Mongo.Interfaces
{

    public interface IImageEntryService
    {
        void Init(string path);
        Task AddAsync(ImageEntry image);
        Task AddOrUpdateAsync(ImageEntry image);
        Task<ImageEntry> FindAsync(ObjectId id);
        Task<List<ImageEntry>> FindAllAsync();
        Task DeleteAsync(ObjectId id);
    }
}
