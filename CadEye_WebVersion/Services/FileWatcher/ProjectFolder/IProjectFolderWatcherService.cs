using FileWatcherEx;
using System.IO;

namespace CadEye_WebVersion.Services.FileWatcher.ProjectFolder
{
    public interface IProjectFolderWatcherService
    {
        void SetupWatcher_repository(FileSystemWatcherEx _watcher);    
    }
}
