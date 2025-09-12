using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.FileSystem;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using MongoDB.Bson;
namespace CadEye_WebVersion.Services.FileWatcher.ProjectFolder
{
    public class ProjectFolderWatcherService : IProjectFolderWatcherService
    {
        private ConcurrentQueue<FileSystemEventArgs> eventQueue_repository = new ConcurrentQueue<FileSystemEventArgs>();
        private readonly IChildFileService _childFileService;

        private IFileSystem _fileSystem;
        public ProjectFolderWatcherService(
            IFileSystem fileSystem,
            IChildFileService childFileService)
        {
            _fileSystem = fileSystem;
            _childFileService = childFileService;
        }

        public void SetupWatcher_repository(FileSystemWatcher _watcher)
        {
            Task.Run(Brdige_Queue_repository);
            _watcher.Created += (s, e) => Bridge_Event_repository(s, e);
            _watcher.EnableRaisingEvents = true;
        }

        private bool isCollecting = false;
        public async Task Accculation(FileSystemEventArgs e)
        {
            List<string> watcherevent = new List<string>();
            DateTime dateTime = DateTime.Now;

            watcherevent.Add(e.FullPath);
            if (!isCollecting)
            {
                if (watcherevent.Count() > 0)
                {
                    isCollecting = true;
                    await Task.Delay(300);


                    isCollecting = false;
                }
            }
        }

        public void Bridge_Event_repository(object sender, FileSystemEventArgs e)
        {
            eventQueue_repository.Enqueue(e);
        }

        public async Task Brdige_Queue_repository()
        {
            while (true)
            {
                if (eventQueue_repository.TryDequeue(out var e))
                {
                    (ObjectId id, string type) = await Repository(e);

                    var node = new ChildFile();

                    string target = string.Empty;
                    var fileinfo = new FileInfo(e.FullPath);

                    target = $"{node.Id}_Created_{DateTime.Now:yyyyMMdd-HHmmss}.dwg";
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }

        private async Task<(ObjectId, string)> Repository(FileSystemEventArgs e)
        {
            ObjectId id = ObjectId.Empty;
            var fileinfo = new FileInfo(e.FullPath);

            var node = await _childFileService.NameFindAsync(e.FullPath);


            byte[] filehash = Hash_Allocated_Unique(e.FullPath);
            var list = await _childFileService.FindAllAsync();

            var matchinghHash = list.FindAll(x => x.HashToken.SequenceEqual(filehash));
            var matchingFileName = matchinghHash.FindAll(x => x.File_Name == Path.GetFileName(e.FullPath));
            var matchingAccesstime = matchingFileName.Find(x => x.AccesTime == fileinfo.LastAccessTime);

            bool existedfile = _fileSystem.isRead(e.FullPath);

            if (node == null)
            {
                if (matchinghHash.Count() == 0) // 생성
                    return (id, "Created");
                else
                {
                    if (matchingAccesstime != null) // 이동
                    {
                        id = matchingAccesstime.Id;
                        return (id, "Moved");
                    }

                    if (matchingAccesstime == null) // 복사
                        return (id, "Copied");
                }
            }
            else
            {
                if (!existedfile) // 삭제
                {
                    id = node.Id;
                    return (id, "Deleted");
                }
                else
                {
                    if (matchinghHash.Count() > 0 && matchingFileName.Count() == 0)
                    {
                        id = node.Id;
                        return (id, "Renamed");
                    }
                    if (matchinghHash.Count() > 0 && matchingFileName.Count() > 0)
                    {
                        id = node.Id;
                        return (id, "Changed");
                    }
                }
            }
            return (id, "Unkown");
        }

        public byte[] Hash_Allocated_Unique(string fullName)
        {
            bool check = _fileSystem.isRead(fullName);
            if (!check) { return Array.Empty<byte>(); }
            {
                var fileName = Path.GetFileName(fullName);

                if (fileName.StartsWith("~$"))
                    return Array.Empty<byte>();

                byte[] contentHash = Array.Empty<byte>();
                int retries = 3;

                while (retries-- > 0)
                {
                    try
                    {
                        using (var stream = new FileStream(fullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var sha = SHA256.Create())
                        {
                            contentHash = sha.ComputeHash(stream);
                        }
                        break;
                    }
                    catch (IOException)
                    {
                        if (retries == 0)
                        {
                            Debug.WriteLine($"파일 잠김: {fullName}");
                            return Array.Empty<byte>();
                        }
                        Task.Delay(100).Wait();
                    }
                }

                string combined = Convert.ToBase64String(contentHash);
                using (var sha = SHA256.Create())
                {
                    return sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combined));

                }
            }
        }
    }
}
