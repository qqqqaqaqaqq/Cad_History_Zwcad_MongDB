using CadEye_WebVersion.Models.Entity;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using System.Threading.Tasks;

namespace CadEye_WebVersion.Infrastructure.Utils
{
    public static class FileCheck
    {
        public static async Task<ConcurrentBag<ChildFile>> AllocateData(string path)
        {
            if (path == null) { return new ConcurrentBag<ChildFile>(); }
            try
            {
                var dirInfo = new DirectoryInfo(path);
                var fileSystemEntries = dirInfo.EnumerateFileSystemInfos("*", SearchOption.AllDirectories)
                    .Where(f => (f.Attributes & FileAttributes.ReparsePoint) == 0)
                    .ToList();
                var semaphore = new SemaphoreSlim(Environment.ProcessorCount);
                var item_insert = new ConcurrentBag<ChildFile>();

                var tasks = fileSystemEntries.Select(async file =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        bool ReadFile = await RetryProvider.RetryAsync(() => Task.FromResult(File.Exists(file.FullName)), 10, 100);
                        if (!ReadFile) return;

                        if (file.Extension.ToUpper() != ".DWG" && file.Extension.ToUpper() != ".DXF") return;

                        var hash = await FileHashProvider.Hash_Allocated_Unique(file.FullName);

                        var sourceNode = await FileInformation(file.FullName);

                        item_insert.Add(sourceNode);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);
                return item_insert;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new ConcurrentBag<ChildFile>();
            }
        }

        public static async Task<ChildFile> FileInformation(string file)
        {
            var fileinfo = new FileInfo(file);
            var hash = await FileHashProvider.Hash_Allocated_Unique(fileinfo.FullName);

            var sourceNode = new ChildFile
            {
                File_FullName = fileinfo.FullName,
                File_Name = fileinfo.Name,
                File_Directory = Path.GetDirectoryName(fileinfo.FullName) ?? "",
                AccesTime = fileinfo.LastAccessTime,
                HashToken = hash
            };

            return sourceNode;
        }
    }
}
