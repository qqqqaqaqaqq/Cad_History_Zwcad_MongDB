using CadEye_WebVersion.Infrastructure.Utils;
using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.Services.PDF;
using MongoDB.Bson;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace CadEye_WebVersion.Services.FileWatcher.RepositoryPdf
{
    public class RepositoryPdfWatcherService : IRepositoryPdfWatcherService
    {
        private ConcurrentQueue<FileSystemEventArgs> eventQueue_repository = new ConcurrentQueue<FileSystemEventArgs>();
        private readonly IImageEntryService _imageEntryService;

        public RepositoryPdfWatcherService(
            IImageEntryService imageEntryService,
            IPdfService pdfService)
        {
            _imageEntryService = imageEntryService;
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

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(AppSettings.PDFTasks);
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
                            if (Path.GetExtension(e.FullPath).ToUpper() == ".PDF")
                            {
                                switch (e.ChangeType)
                                {
                                    case WatcherChangeTypes.Created:
                                        await Repository(e);
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"예외 발생: {ex}");
                        }
                        finally
                        {
                            _semaphore.Release(); // 반드시 슬롯 반환
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

            ObjectId objectId = ObjectId.Parse(_id);
            DateTime time = DateTime.ParseExact(_time, "yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);

            await ImagesInsert(objectId, time, e.FullPath);
        }

        public async Task ImagesInsert(ObjectId objectId, DateTime time, string path)
        {
            var child_node = new List<ImageTimePath>();

            var node = await _imageEntryService.FindAsync(objectId);
            if (node != null)
            {
                child_node = node.Path;
            }

            child_node.Add(new ImageTimePath()
            {
                Time = time,
                ImagePath = path
            });

            var insert_node = new ImageEntry()
            {
                Id = objectId,
                Path = child_node
            };

            await _imageEntryService.AddOrUpdateAsync(insert_node);
        }
    }
}
