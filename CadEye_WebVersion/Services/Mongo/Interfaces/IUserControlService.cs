using CadEye_WebVersion.Models;

namespace CadEye_WebVersion.Services.Mongo.Interfaces
{
    public interface IUserControlService
    {

        // 어드민 관리자 엔트리
        void Init();
        Task UpdateAsync(AdminEntity evt);
        Task AddAsync(AdminEntity evt);
        Task DeleteAsync(string googleId);
        Task<AdminEntity> FindAsync(string googleId);
        Task AddOrUpdateAsync(AdminEntity evt);
    }
}
