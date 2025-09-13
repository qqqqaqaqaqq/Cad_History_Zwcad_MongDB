using System.Diagnostics;
using System.IO;

namespace CadEye_WebVersion.Infrastructure.Utils
{
    public static class SetWatcher
    {
        public static FileSystemWatcher StartWatcher(ref FileSystemWatcher _watcher, string path)
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null!;
            }
            _watcher = new FileSystemWatcher(path)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.CreationTime
                             | NotifyFilters.FileName
                             | NotifyFilters.LastAccess
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.LastWrite
            };
            _watcher.InternalBufferSize = 64 * 1024;

            return _watcher;
        }
    }
}
