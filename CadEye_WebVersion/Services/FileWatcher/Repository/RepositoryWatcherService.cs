using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.FileSystem;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.Services.Zwcad;
using MongoDB.Bson;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace CadEye_WebVersion.Services.FileWatcher.Repository
{
    public class RepositoryWatcherService : IRepositoryWatcherService
    {
        private ConcurrentQueue<FileSystemEventArgs> eventQueue_repository = new ConcurrentQueue<FileSystemEventArgs>();
        private readonly IEventEntryService _eventEntryService;
        private readonly IRefEntryService _refEntryService;
        private readonly IZwcadService _zwcadService;


        private IFileSystem _fileSystem;
        public RepositoryWatcherService(IFileSystem fileSystem, IEventEntryService eventEntryService, IZwcadService zwcadService, IRefEntryService refEntryService)
        {
            _fileSystem = fileSystem;
            _eventEntryService = eventEntryService;
            _zwcadService = zwcadService;
            _refEntryService = refEntryService;
        }

        public void SetupWatcher_repository(FileSystemWatcher _watcher)
        {
            Task.Run(Brdige_Queue_repository);
            _watcher.Created += (s, e) => Bridge_Event_repository(s, e);
            _watcher.EnableRaisingEvents = true;
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
                    switch (e.ChangeType)
                    {
                        case WatcherChangeTypes.Created:
                            Repository(e);
                            break;
                    }
                    await Task.Delay(100);
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }

        private readonly object _lock = new object();
        private async void Repository(FileSystemEventArgs e)
        {
            _fileSystem.isRead(e.FullPath);

            string[] parts = Path.GetFileNameWithoutExtension(e.FullPath).Split("_");
            string _id = parts[0];
            string _event = parts[1];
            string _time = parts[2];

            ObjectId objectId = ObjectId.Parse(_id);
            DateTime time = DateTime.ParseExact(_time, "yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);

            await EventInsert(objectId, time, _event);

            await RefsInsert(objectId, time, e.FullPath);
        }

        public async Task EventInsert(ObjectId objectId, DateTime time, string eventname)
        {
            var child_node = new List<EventList>();

            var node = await _eventEntryService.FindAsync(objectId);

            if (node != null)
            {
                child_node = node.EventCollection;
            }

            child_node.Add(new EventList()
            {
                Time = time,
                EventName = eventname
            });

            var insert_node = new EventEntry()
            {
                Id = objectId,
                EventCollection = child_node,
            };

            await _eventEntryService.AddOrUpdateAsync(insert_node);
        }

        public async Task RefsInsert(ObjectId objectId, DateTime time, string path)
        {
            var list = new List<string>();
            lock (_lock)
            {
                list = _zwcadService.WorkFlow_Zwcad(path);
            }

            var child_node = new List<RefsList>();

            var node = await _refEntryService.FindAsync(objectId);
            if (node != null)
            {
                child_node = node.Refs;
            }

            child_node.Add(new RefsList()
            {
                Time = time,
                Ref = list
            });

            var insert_node = new RefEntry()
            {
                Id = objectId,
                Refs = child_node,
            };

            await _refEntryService.AddOrUpdateAsync(insert_node);
        }
    }
}
