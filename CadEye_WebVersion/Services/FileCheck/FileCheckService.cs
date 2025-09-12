using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using CadEye_WebVersion.Services.FileSystem;

namespace CadEye_WebVersion.Services.FileCheck
{
    public class FileCheckService : IFileCheckService
    {
        private readonly IChildFileService _mongoService;
        private readonly IFileSystem _fileSystem;
        public FileCheckService(
            IChildFileService mongoService,
            IFileSystem fileSystem)
        {
            _mongoService = mongoService;
            _fileSystem = fileSystem;
        }

        public async Task<ConcurrentBag<ChildFile>> AllocateData(string path)
        {
            if (path == null) { return new ConcurrentBag<ChildFile>(); }
            try
            {
                var dirInfo = new DirectoryInfo(path);
                var fileSystemEntries = dirInfo.EnumerateFileSystemInfos("*", SearchOption.AllDirectories)
                    .Where(f => (f.Attributes & FileAttributes.ReparsePoint) == 0)
                    .ToList();
                var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
                var item_insert = new ConcurrentBag<ChildFile>();

                var mongodatas = await _mongoService.FindAllAsync().ConfigureAwait(false);

                var data = new HashSet<string>(mongodatas.Select(f => f.File_FullName + Convert.ToBase64String(f.HashToken)));


                Parallel.ForEach(fileSystemEntries, options, file =>
                {
                    if (file.Extension.ToUpper() != ".DWG" && file.Extension.ToUpper() != ".DXF")
                    {
                        return;
                    }

                    if ((file.Attributes & FileAttributes.Directory) == 0)
                    {
                        byte[] hash = Hash_Allocated_Unique(file.FullName);
                        string key = file.FullName + Convert.ToBase64String(hash);

                        if (data.Contains(key)) return;

                        var source_node = new ChildFile();
                        source_node.File_FullName = file.FullName;
                        source_node.File_Name = file.Name;
                        source_node.File_Directory = Path.GetDirectoryName(file.FullName) ?? "";
                        source_node.AccesTime = file.LastAccessTime;
                        source_node.HashToken = hash;

                        item_insert.Add(source_node);
                    }
                });

                return item_insert;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new ConcurrentBag<ChildFile>();
            }
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
