using CadEye_WebVersion.Infrastructure.Utils;
using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using MongoDB.Bson;

namespace CadEye_WebVersion.Services.FileWatcher.ProjectFolder
{
    public class ProjectFolderWatcherService : IProjectFolderWatcherService
    {
        private ConcurrentQueue<FileSystemEventArgs> eventQueue_repository = new ConcurrentQueue<FileSystemEventArgs>();
        private readonly IChildFileService _childFileService;

        public ProjectFolderWatcherService(
            IChildFileService childFileService)
        {
            _childFileService = childFileService;
        }

        private readonly Subject<FileSystemEventArgs> _subject = new Subject<FileSystemEventArgs>();

        public void SetupWatcher_repository(FileSystemWatcher _watcher)
        {
            Task.Run(Brdige_Queue_repository);
            _subject
                .Subscribe(evt =>
                {
                    eventQueue_repository.Enqueue(evt);
                });

            _watcher.Created += (s, e) => _subject.OnNext(e);
            _watcher.Changed += (s, e) => _subject.OnNext(e);
            _watcher.Deleted += (s, e) => _subject.OnNext(e);
            _watcher.Renamed += (s, e) => _subject.OnNext(e);

            _watcher.EnableRaisingEvents = true;
        }

        public async Task Brdige_Queue_repository()
        {
            while (true)
            {
                if (eventQueue_repository.TryDequeue(out var e))
                {
                    if (!await ExtFilterProvider.ExtFilter(e.FullPath)) continue; // 확장자 필터

                    var Event = e.ChangeType;
                    string target = string.Empty;
                    var source_node = new ChildFile();
                    var original_node = new ChildFile();
                    string Description = string.Empty;

                    Debug.WriteLine(e.FullPath);
                    if (await RetryProvider.RetryAsync(() => Task.FromResult(File.Exists(e.FullPath)), 100, 100)) // 파일 읽을 수 있는지?
                    {
                        var fileinfo = new FileInfo(e.FullPath);
                        var hash = await FileHashProvider.Hash_Allocated_Unique(e.FullPath);
                        switch (Event)
                        {
                            case WatcherChangeTypes.Created:
                                source_node.File_FullName = e.FullPath;
                                source_node.File_Name = Path.GetFileName(e.FullPath);
                                source_node.File_Directory = Path.GetDirectoryName(e.FullPath) ?? "";
                                source_node.HashToken = hash;
                                await _childFileService.AddAsync(source_node);
                                break;
                            case WatcherChangeTypes.Changed:
                                original_node = await _childFileService.NameFindAsync(e.FullPath);
                                source_node.Id = original_node.Id;
                                source_node.File_FullName = e.FullPath;
                                source_node.File_Name = Path.GetFileName(e.FullPath);
                                source_node.File_Directory = Path.GetDirectoryName(e.FullPath) ?? "";
                                source_node.HashToken = hash;
                                await _childFileService.AddOrUpdateAsync(source_node);
                                break;
                            case WatcherChangeTypes.Renamed:
                                RenamedEventArgs re = e as RenamedEventArgs;
                                if (re == null) break;
                                original_node = await _childFileService.NameFindAsync(re.OldFullPath);
                                source_node.Id = original_node.Id;
                                source_node.File_FullName = e.FullPath;
                                source_node.File_Name = Path.GetFileName(e.FullPath);
                                source_node.File_Directory = Path.GetDirectoryName(e.FullPath) ?? "";
                                source_node.HashToken = hash;
                                Description = $"{Path.GetFileName(re.OldFullPath)}";
                                await _childFileService.AddOrUpdateAsync(source_node);
                                break;
                            case WatcherChangeTypes.Deleted:
                                continue;
                        }
                        var chknode = await _childFileService.NameFindAsync(e.FullPath);
                        target = $"{chknode.Id}_{Event.ToString()}_{DateTime.Now:yyyyMMdd-HHmmss}_{Description}.dwg";
                        target = System.IO.Path.Combine(AppSettings.RepositoryDwgFolder!, target);
                        await RetryProvider.RetryAsync(() => FileCopyProvider.FileCopy(e.FullPath, target), 10, 200);
                    }
                    else
                    {
                        var chknode = await _childFileService.NameFindAsync(e.FullPath);
                        target = $"{chknode.Id}_{Event.ToString()}_{DateTime.Now:yyyyMMdd-HHmmss}_.dwg";
                        target = System.IO.Path.Combine(AppSettings.RepositoryDwgFolder!, target);
                        await RetryProvider.RetryAsync(() =>
                        { 
                            using var fs = File.Create(target);
                            return Task.FromResult(true);
                        }
                        , 10
                        , 200);
                    }
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }
    }
}
