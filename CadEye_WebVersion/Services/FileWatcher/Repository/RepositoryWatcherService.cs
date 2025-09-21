using CadEye_WebVersion.Infrastructure.Utils;
using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.Services.Zwcad;
using MongoDB.Bson;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace CadEye_WebVersion.Services.FileWatcher.Repository
{
    public class RepositoryWatcherService : IRepositoryWatcherService
    {
        private ConcurrentQueue<FileSystemEventArgs> eventQueue_repository = new ConcurrentQueue<FileSystemEventArgs>();
        private readonly IEventEntryService _eventEntryService;
        private readonly IRefEntryService _refEntryService;
        private readonly IZwcadService _zwcadService;


        public RepositoryWatcherService(
            IEventEntryService eventEntryService,
            IZwcadService zwcadService,
            IRefEntryService refEntryService)
        {
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

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(AppSettings.ZwcadThreads);

        public async Task Brdige_Queue_repository()
        {
            while (true)
            {
                if (eventQueue_repository.TryDequeue(out var e))
                {
                    await _semaphore.WaitAsync(); 

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            switch (e.ChangeType)
                            {
                                case WatcherChangeTypes.Created:
                                    await Repository(e);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"예외 발생: {ex}");
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    });
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }

        private async Task Repository(FileSystemEventArgs e)
        {
            bool ReadFile = await RetryProvider.RetryAsync(() => Task.FromResult(File.Exists(e.FullPath)), 100, 100);

            string[] parts = Path.GetFileNameWithoutExtension(e.FullPath).Split("_");
            string _id = parts[0];
            string _event = parts[1];
            string _time = parts[2];
            string _description = parts[3];

            if (string.IsNullOrEmpty(_description)) _description = "";

            ObjectId objectId = ObjectId.Parse(_id);
            DateTime time = DateTime.ParseExact(_time, "yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);

            await EventInsert(objectId, time, _event, _description);

            await RefsInsert(objectId, time, e.FullPath);
        }

        public async Task EventInsert(ObjectId objectId, DateTime time, string eventname, string description)
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
                EventName = eventname,
                EventDescription = description

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

            list = _zwcadService.WorkFlow_Zwcad(path);

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
