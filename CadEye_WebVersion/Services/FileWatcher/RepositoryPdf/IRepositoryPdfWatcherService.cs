using System.IO;

namespace CadEye_WebVersion.Services.FileWatcher.RepositoryPdf
{
    public interface IRepositoryPdfWatcherService
    {
        void SetupWatcher_repository(FileSystemWatcher _watcher);    
    }
}
