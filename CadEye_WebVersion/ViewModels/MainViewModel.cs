using CadEye_WebVersion.Commands;
using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.FileCheck;
using CadEye_WebVersion.Services.FileSystem;
using CadEye_WebVersion.Services.FileWatcher.Repository;
using CadEye_WebVersion.Services.FileWatcher.RepositoryPdf;
using CadEye_WebVersion.Services.FolderService;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.Services.PDF;
using CadEye_WebVersion.ViewModels.Messages;
using CommunityToolkit.Mvvm.Messaging;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CadEye_WebVersion.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly iFolderService _folderService;
        private readonly IFileCheckService _fileCheckService;
        public readonly IChildFileService _childFileService;
        public readonly IEventEntryService _eventEntryService;
        public readonly IImageEntryService _imageEntryService;
        public readonly IRefEntryService _refEntryService;
        public readonly IFileSystem _fileSystem;
        public readonly IRepositoryPdfWatcherService _pdfWatcherService;
        public readonly IRepositoryWatcherService _dwgWatcherService;
        private string _projectName = string.Empty;
        private ObservableCollection<CadEye_WebVersion.Models.TreeNode>? _fileList;
        public readonly IPdfService _pdfService;
        public FileSystemWatcher? _watcher_repository_dwg;
        public FileSystemWatcher? _watcher_repository_pdf;

        private System.Windows.Media.Brush _globalcolor = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#fff"));

        private string _statusMessage { get; set; } = "Status";

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public System.Windows.Media.Brush GlobalColor
        {
            get => _globalcolor;
            set
            {
                _globalcolor = value;
                OnPropertyChanged();
            }
        }

        public string ProjectName
        {
            get => _projectName;
            set
            {
                _projectName = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<CadEye_WebVersion.Models.TreeNode>? FileList
        {
            get => _fileList;
            set
            {
                _fileList = value;
                OnPropertyChanged();
            }
        }

        private CadEye_WebVersion.Models.TreeNode? _selectedTreeNode;
        public CadEye_WebVersion.Models.TreeNode? SelectedTreeNode
        {
            get => _selectedTreeNode;
            set
            {
                if (_selectedTreeNode != value)
                {
                    _selectedTreeNode = value;
                    OnPropertyChanged(nameof(SelectedTreeNode));

                    if (_selectedTreeNode != null)
                    {
                        var clickedId = _selectedTreeNode.Id;

                        if (_selectedTreeNode != null)
                            WeakReferenceMessenger.Default.Send(new SelectedTreeNodeMessage(_selectedTreeNode.Id));
                    }
                }
            }
        }

        public MainViewModel(
            iFolderService folderService,
            IFileCheckService fileCheckService,
            IChildFileService childFileService,
            IEventEntryService eventEntryService,
            IImageEntryService imageEntryService,
            IRefEntryService refEntryService,
            IPdfService pdfService,
            IFileSystem fileSystem,
            IRepositoryPdfWatcherService fileWatcherService,
            IRepositoryWatcherService dwgWatcherService
            )
        {
            FolderSelecter = new FolderSelect(folderService, OnFolderSelected);
            _folderService = folderService;
            _fileCheckService = fileCheckService;
            _childFileService = childFileService;
            _eventEntryService = eventEntryService;
            _imageEntryService = imageEntryService;
            _refEntryService = refEntryService;
            _pdfService = pdfService;
            _fileSystem = fileSystem;
            _pdfWatcherService = fileWatcherService;
            _dwgWatcherService = dwgWatcherService;

            WeakReferenceMessenger.Default.Register<SendStatusMessage>(this, async (r, m) =>
            {
                await SatatusUpdate(m.Value);
            });
        }

        private async Task SatatusUpdate(string status)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                StatusMessage = status;
            });
        }

        public async void OnFolderSelected(string path)
        {
            StatusMessage = "Start...";
            await Task.Delay(100);
            ProjectName = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(ProjectName))
                return;

            string dbName = ProjectName.Trim().Replace(" ", "_").Replace(".", "_");

            if (string.IsNullOrEmpty(dbName))
            {
                System.Windows.MessageBox.Show("올바른 경로가 아닙니다.", "알림");
                StatusMessage = "Project Name Failure";
                return;
            }

            StatusMessage = "Project Name Completed";


            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase(dbName);

            _childFileService.Init(dbName);
            _eventEntryService.Init(dbName);
            _imageEntryService.Init(dbName);
            _refEntryService.Init(dbName);

            StatusMessage = "Mongo Set Completed";
            await Task.Delay(100);

            ConcurrentBag<ChildFile> file_list = new ConcurrentBag<ChildFile>();
            try
            {
                file_list = await _fileCheckService.AllocateData(path);

                if (file_list.Count > 0)
                {
                    await _childFileService.AddAllAsync(file_list.ToList());
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                StatusMessage = "File Research Failure";
                return;
            }
            StatusMessage = "File Research Completed";

            await Task.Delay(100);

            // Repository 폴더 셋팅
            string currentFolder = AppDomain.CurrentDomain.BaseDirectory;
            string repositoryDwgPath = System.IO.Path.Combine(currentFolder, "repository", dbName);
            if (!System.IO.Directory.Exists(repositoryDwgPath))
            {
                System.IO.Directory.CreateDirectory(repositoryDwgPath);
            }
            string repositoryPdfPath = System.IO.Path.Combine(currentFolder, "repository", $"{dbName}_pdf");
            if (!System.IO.Directory.Exists(repositoryPdfPath))
            {
                System.IO.Directory.CreateDirectory(repositoryPdfPath);
            }

            AppSettings.RepositoryPdfFolder = repositoryPdfPath;


            // 폴더 존재 여부 체크
            int retry = 10;
            bool foldercheck = false;
            while (retry-- > 0)
            {
                try
                {
                    Directory.Exists(repositoryPdfPath);
                    Directory.Exists(repositoryDwgPath);
                    foldercheck = true;
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    await Task.Delay(100);
                }
            }

            if (!foldercheck)
            {
                StatusMessage = "Folder Setting Failure";
                return;
            }

            StatusMessage = "Folder Setting Completed";


            // 파일 와처 셋팅 성공 여부 체크
            bool watchercheck = false;
            watchercheck = StartWatcher(ref _watcher_repository_dwg, repositoryDwgPath, "dwg");

            if (!watchercheck)
            {
                StatusMessage = "Folder Monitoring Setting Failure";
                return;
            }

            watchercheck = StartWatcher(ref _watcher_repository_pdf, repositoryPdfPath, "pdf");

            if (!watchercheck)
            {
                StatusMessage = "Folder Monitoring Setting Failure";
                return;
            }

            StatusMessage = "Folder Monitoring Setting Completed";


            var allFiles = await _childFileService.FindAllAsync();

            if (file_list.Count > 0)
            {
                foreach (var file in allFiles)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(repositoryDwgPath)) { return; }
                        string target = System.IO.Path.Combine(repositoryDwgPath, $"{file.Id}_Created_{DateTime.Now:yyyyMMdd-HHmmss}.dwg");
                        await _fileSystem.FileCopy(file.File_FullName, target);
                        await Task.Delay(100);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        StatusMessage = $"{file} Copy Failure";
                        await Task.Delay(100);
                    }
                }
            }

            StatusMessage = "File Repository Copy Completed";

            bool treecreated = TreeNodeCreate(allFiles, path);

            if (!treecreated)
                StatusMessage = "FileList View Failure";

            StatusMessage = "FileList View Completed";
        }

        public bool TreeNodeCreate(List<ChildFile> allFiles, string path)
        {
            try
            {
                FileList = BuildTreeFromFiles(allFiles, path);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public bool StartWatcher(ref FileSystemWatcher _watcher, string path, string folder)
        {
            try
            {
                if (_watcher != null)
                {
                    _watcher.EnableRaisingEvents = false;
                    _watcher.Dispose();
                    _watcher = null;
                }
                _watcher = new FileSystemWatcher(path)
                {
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.CreationTime
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastWrite
                };
                _watcher.InternalBufferSize = 64 * 1024;

                switch (folder)
                {
                    case "dwg":
                        _dwgWatcherService.SetupWatcher_repository(_watcher);
                        break;
                    case "pdf":
                        _pdfWatcherService.SetupWatcher_repository(_watcher);
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        private ObservableCollection<CadEye_WebVersion.Models.TreeNode> BuildTreeFromFiles(List<ChildFile> files, string projectRootPath)
        {
            var root = new CadEye_WebVersion.Models.TreeNode
            {
                Name = ProjectName,
            };

            foreach (var file in files)
            {
                string relativePath = System.IO.Path.GetRelativePath(projectRootPath, file.File_FullName);

                var parts = relativePath.Split(
                    new[] {
                    System.IO.Path.DirectorySeparatorChar,
                    System.IO.Path.AltDirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries);

                var current = root;

                foreach (var part in parts)
                {
                    var child = current.Children.FirstOrDefault(c => c.Name == part);
                    if (child == null)
                    {
                        child = new CadEye_WebVersion.Models.TreeNode
                        {
                            Name = part
                        };

                        if (part == parts.Last())
                        {
                            child.Id = file.Id;
                        }
                        current.Children.Add(child);
                    }
                    current = child;
                }
            }

            return new ObservableCollection<CadEye_WebVersion.Models.TreeNode> { root };
        }

        public FolderSelect FolderSelecter { get; }
        public iFolderService FolderService => _folderService;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

