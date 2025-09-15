using CadEye_WebVersion.Services.Google.Models;

namespace CadEye_WebVersion.Services.Mongo.Interfaces
{
    public interface IUserControlService
    {
        void Init(string path);
        Task AddAsync(LoginEntity evt);
        Task DeleteAsync(string googleId);
        Task<LoginEntity> FindAsync(string googleId);
    }
}
