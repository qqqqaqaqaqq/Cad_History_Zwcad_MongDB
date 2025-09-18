using CadEye_WebVersion.Services.Google.Models;

namespace CadEye_WebVersion.Services.Mongo.Interfaces
{
    public interface IUserControlService
    {

        // 어드민 관리자 엔트리
        void Init(string path);
        Task AddAsync(LoginEntity evt);
        Task DeleteAsync(string googleId);
        Task<LoginEntity> FindAsync(string googleId);
    }
}
