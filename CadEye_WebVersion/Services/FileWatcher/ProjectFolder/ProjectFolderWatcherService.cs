using CadEye_WebVersion.Infrastructure.Utils;
using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FileWatcherEx;

namespace CadEye_WebVersion.Services.FileWatcher.ProjectFolder
{
    public class ProjectFolderWatcherService : IProjectFolderWatcherService
    {
        private ConcurrentQueue<FileChangedEvent> eventQueue_repository = new ConcurrentQueue<FileChangedEvent>();
        private readonly IChildFileService _childFileService;

        public ProjectFolderWatcherService(
            IChildFileService childFileService)
        {
            _childFileService = childFileService;
        }

        private readonly Subject<FileWatcherEx.FileChangedEvent> _subject = new Subject<FileChangedEvent>();

        public void SetupWatcher_repository(FileSystemWatcherEx _watcher)
        {
            Task.Run(Brdige_Queue_repository);
            _subject
                .Subscribe(evt =>
                {
                    eventQueue_repository.Enqueue(evt);
                });

            _watcher.OnCreated += (s, e) => _subject.OnNext(e);
            _watcher.OnChanged += (s, e) => _subject.OnNext(e);
            _watcher.OnDeleted += (s, e) => _subject.OnNext(e);
            _watcher.OnRenamed += (s, e) => _subject.OnNext(e);

            _watcher.IncludeSubdirectories = true;
            _watcher.Start();
        }

        public async Task Brdige_Queue_repository()
        {
            while (true)
            {
                if (eventQueue_repository.TryDequeue(out var e))
                {
                    if (!await ExtFilterProvider.ExtFilter(e.FullPath)) continue;

                    var Event = e.ChangeType;
                    string target = string.Empty;
                    var source_node = new ChildFile();
                    var original_node = new ChildFile();
                    string Description = string.Empty;

                    FileInfo fileinfo;
                    var hash = new byte[] { };

                    if (e.ChangeType != ChangeType.DELETED)
                    {
                        fileinfo = new FileInfo(e.FullPath);
                        hash = await FileHashProvider.Hash_Allocated_Unique(e.FullPath);
                    }
                    switch (Event)
                    {
                        case ChangeType.CREATED:
                            source_node.File_FullName = e.FullPath;
                            source_node.File_Name = Path.GetFileName(e.FullPath);
                            source_node.File_Directory = Path.GetDirectoryName(e.FullPath) ?? "";
                            source_node.HashToken = hash;
                            await _childFileService.AddAsync(source_node);
                            break;
                        case ChangeType.CHANGED:
                            original_node = await _childFileService.NameFindAsync(e.FullPath);
                            source_node.Id = original_node.Id;
                            source_node.File_FullName = e.FullPath;
                            source_node.File_Name = Path.GetFileName(e.FullPath);
                            source_node.File_Directory = Path.GetDirectoryName(e.FullPath) ?? "";
                            source_node.HashToken = hash;
                            await _childFileService.AddOrUpdateAsync(source_node);
                            break;
                        case ChangeType.RENAMED:
                            var re = e;
                            if (string.IsNullOrEmpty(re.OldFullPath)) break;
                            original_node = await _childFileService.NameFindAsync(re.OldFullPath);
                            if (original_node == null) break;
                            source_node.Id = original_node.Id;
                            source_node.File_FullName = re.FullPath;
                            source_node.File_Name = Path.GetFileName(re.FullPath);
                            source_node.File_Directory = Path.GetDirectoryName(re.FullPath) ?? "";
                            source_node.HashToken = hash;
                            Description = $"Renamed from {Path.GetFileName(re.OldFullPath)} to {source_node.File_Name}";
                            await _childFileService.AddOrUpdateAsync(source_node);
                            break;
                        case ChangeType.DELETED:
                            original_node = await _childFileService.NameFindAsync(e.FullPath);
                            if (original_node == null) break;
                            source_node.Id = original_node.Id;
                            source_node.File_FullName = e.FullPath;
                            source_node.File_Name = Path.GetFileName(e.FullPath);
                            source_node.File_Directory = Path.GetDirectoryName(e.FullPath) ?? "";
                            await _childFileService.AddOrUpdateAsync(source_node);
                            continue;
                    }

                    var chknode = await _childFileService.NameFindAsync(e.FullPath);
                    target = $"{chknode.Id}_{Event.ToString()}_{DateTime.Now:yyyyMMdd-HHmmss}_{Description}.dwg";
                    target = System.IO.Path.Combine(AppSettings.RepositoryDwgFolder!, target);
                    await RetryProvider.RetryAsync(() => FileCopyProvider.FileCopy(e.FullPath, target), 10, 200);
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }
    }
}
