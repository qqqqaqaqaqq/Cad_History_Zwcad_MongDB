using System.IO;

namespace CadEye_WebVersion.Services.FileWatcher.Repository
{
    public interface IRepositoryWatcherService
    {
        void SetupWatcher_repository(FileSystemWatcher _watcher);    
    }
}
