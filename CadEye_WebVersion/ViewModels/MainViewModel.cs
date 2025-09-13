using CadEye_WebVersion.Commands;
using CadEye_WebVersion.Infrastructure.Utils;
using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.FileWatcher.ProjectFolder;
using CadEye_WebVersion.Services.FileWatcher.Repository;
using CadEye_WebVersion.Services.FileWatcher.RepositoryPdf;
using CadEye_WebVersion.Services.FolderService;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.Services.PDF;
using CadEye_WebVersion.ViewModels.Messages;
using CommunityToolkit.Mvvm.Messaging;
using CadEye_WebVersion.Models;
using MongoDB.Driver;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using CadEye_WebVersion.ViewModels.Messages.ThemeBrush;

namespace CadEye_WebVersion.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly iFolderService _folderService;
        public readonly IChildFileService _childFileService;
        public readonly IEventEntryService _eventEntryService;
        public readonly IImageEntryService _imageEntryService;
        public readonly IRefEntryService _refEntryService;
        public readonly IProjectFolderWatcherService _projectFolderWatcherService;
        public readonly IRepositoryPdfWatcherService _pdfWatcherService;
        public readonly IRepositoryWatcherService _dwgWatcherService;
        private ObservableCollection<Models.FileTreeNode>? _fileList;
        public readonly IPdfService _pdfService;
        public FileSystemWatcher? _watcher_repository_project;
        public FileSystemWatcher? _watcher_repository_dwg;
        public FileSystemWatcher? _watcher_repository_pdf;

        private System.Windows.Media.Brush _globalcolor = 
            new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#80EEEEEE"));
        private System.Windows.Media.Brush? _theme = 
            new SolidColorBrush((System.Windows.Media.Color) System.Windows.Media.ColorConverter.ConvertFromString("#000"));
        private System.Windows.Media.Brush? _viewCotainBackGround = 
            new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#fff"));
        private System.Windows.Media.Brush? _globalBorderBrush =
            new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFCCCCCC"));

        private string _statusMessage { get; set; } = "Status";

        private string? _projectname;

        public System.Windows.Media.Brush? GlobalBorderBrush
        {
            get => _globalBorderBrush;
            set
            {
                if (_globalBorderBrush != value)
                {
                    _globalBorderBrush = value;
                    OnPropertyChanged();
                }
            }
        }

        public System.Windows.Media.Brush? ViewCotainBackGround
        {
            get => _viewCotainBackGround;
            set
            {
                if (_viewCotainBackGround != value)
                {
                    _viewCotainBackGround = value;
                    OnPropertyChanged();
                }
            }
        }

        public System.Windows.Media.Brush? Theme
        {
            get => _theme;
            set
            {
                if (_theme != value)
                {
                    _theme = value;
                    OnPropertyChanged();
                }
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
            get => _projectname ?? "Project Name";
            set
            {
               _projectname = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FileTreeNode>? FileList
        {
            get => _fileList;
            set
            {
                _fileList = value;
                OnPropertyChanged();
            }
        }

        private Models.FileTreeNode? _selectedTreeNode;
        public Models.FileTreeNode? SelectedTreeNode
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

        private ImageSource? _folderIcon;

        public ImageSource? FolderIcon
        {
            get => _folderIcon;
            set
            {
                if (_folderIcon != value)
                {
                    _folderIcon = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainViewModel(
            iFolderService folderService,
            IChildFileService childFileService,
            IEventEntryService eventEntryService,
            IImageEntryService imageEntryService,
            IRefEntryService refEntryService,
            IPdfService pdfService,
            IRepositoryPdfWatcherService fileWatcherService,
            IRepositoryWatcherService dwgWatcherService,
            IProjectFolderWatcherService projectFolderWatcherService
            )
        {
            FolderIcon = SystemIconProvider.FolderIcon();
            FolderSelecter = new FolderSelect(folderService, OnFolderSelected);
            TreeRefreshHandler = new TreeRefresh(childFileService);
            _folderService = folderService;
            _childFileService = childFileService;
            _eventEntryService = eventEntryService;
            _imageEntryService = imageEntryService;
            _refEntryService = refEntryService;
            _pdfService = pdfService;
            _pdfWatcherService = fileWatcherService;
            _dwgWatcherService = dwgWatcherService;
            _projectFolderWatcherService = projectFolderWatcherService;

            WeakReferenceMessenger.Default.Register<SendStatusMessage>(this, async (r, m) =>
            {
                await SatatusUpdate(m.Value);
            });

            WeakReferenceMessenger.Default.Register<SendFileTreeNode>(this, async (r, m) =>
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    FileList = new ObservableCollection<FileTreeNode> { m.Value };
                });
            });
            WeakReferenceMessenger.Default.Register<SendGlobalColor>(this, async (r, m) =>
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                   GlobalColor = m.Value;
                });
            });
            WeakReferenceMessenger.Default.Register<SendViewContainBackGround>(this, async (r, m) =>
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ViewCotainBackGround = m.Value;
                });
            });
            WeakReferenceMessenger.Default.Register<SendGlobalBorderBrush>(this, async (r, m) =>
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    GlobalBorderBrush = m.Value;
                });
            });
            WeakReferenceMessenger.Default.Register<SendForeGroundBrush>(this, async (r, m) =>
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Theme = m.Value;
                });
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

            // 프로젝트 네임 셋팅
            string projectname = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(projectname))
                return;

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                ProjectName = projectname;
            });

            AppSettings.ProjectPath = path;
            string dbName = projectname.Trim().Replace(" ", "_").Replace(".", "_");

            if (string.IsNullOrEmpty(dbName))
            {
                System.Windows.MessageBox.Show("올바른 경로가 아닙니다.", "알림");
                StatusMessage = "Project Name Failure";
                return;
            }

            StatusMessage = "Project Name Completed";

            // Mongo collection 셋팅
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase(dbName);

            _childFileService.Init(dbName);
            _eventEntryService.Init(dbName);
            _imageEntryService.Init(dbName);
            _refEntryService.Init(dbName);

            StatusMessage = "Mongo Set Completed";

            var allFiles = await _childFileService.FindAllAsync() ?? new List<ChildFile>();
            var filtered = new List<ChildFile>();

            // 감시 폴더 내 파일 가져오기
            try
            {
                var file_list = await FileCheck.AllocateData(path);

                var existingFiles = new HashSet<string>(allFiles.Select(x => x.File_FullName));

                filtered = file_list
                    .Where(f => !existingFiles.Contains(f.File_FullName))
                    .ToList();

                if (filtered.Count() > 0)
                {
                    await _childFileService.AddAllAsync(filtered);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                StatusMessage = "File Research Failure";
                return;
            }
            StatusMessage = "File Research Completed";

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
            AppSettings.RepositoryDwgFolder = repositoryDwgPath;

            // 폴더 존재 여부 체크
            bool RetrySuccess = false;

            RetrySuccess = await RetryProvider.Retry(() => Directory.Exists(repositoryPdfPath), 100, 100);
            RetrySuccess = await RetryProvider.Retry(() => Directory.Exists(repositoryDwgPath), 100, 100);

            if (!RetrySuccess)
            {
                StatusMessage = "Folder Setting Failure";
                return;
            }
            StatusMessage = "Folder Setting Completed";

            // 파일 와처 셋팅
            var _watcher = SetWatcher.StartWatcher(ref _watcher_repository_project!, path);
            try
            {
                _projectFolderWatcherService.SetupWatcher_repository(_watcher);
                _watcher.EnableRaisingEvents = true;
            }
            catch
            {
                StatusMessage = "ProjectFolder Monitoring Failure";
                return;
            }
            StatusMessage = "ProjectFolder Monitoring Completed";

            _watcher = SetWatcher.StartWatcher(ref _watcher_repository_dwg!, repositoryDwgPath);
            try
            {
                _dwgWatcherService.SetupWatcher_repository(_watcher);
                _watcher.EnableRaisingEvents = true;
            }
            catch
            {
                StatusMessage = "Repository folder Monitoring  Failure";
                return;
            }
            StatusMessage = "Repository folder Monitoring  Completed";

            _watcher = SetWatcher.StartWatcher(ref _watcher_repository_pdf!, repositoryPdfPath);
            try
            {
                _pdfWatcherService.SetupWatcher_repository(_watcher);
                _watcher.EnableRaisingEvents = true;
            }
            catch
            {
                StatusMessage = "Repository Pdf Folder Monitoring Failure";
                return;
            }
            StatusMessage = "Repository Pdf Folder Monitoring Completed";

            // 파일 복사
            allFiles = await _childFileService.FindAllAsync() ?? new List<ChildFile>();
            if (filtered.Count() > 0)
            {
                var task = filtered.Select(async file =>
                {
                    string target = System.IO.Path.Combine(repositoryDwgPath, $"{file.Id}_Created_{DateTime.Now:yyyyMMdd-HHmmss}_Start.dwg");
                    bool retrysuccess = await RetryProvider.RetryAsync(() => FileCopyProvider.FileCopy(file.File_FullName, target), 10, 200);

                    if (!retrysuccess)
                    {
                        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            StatusMessage = $"{file} Copy Failed";
                        });
                    }
                });

                await Task.WhenAll(task);
            }
            StatusMessage = "File Repository Copy Completed";

            // TreeView 작성
            allFiles = await _childFileService.FindAllAsync() ?? new List<ChildFile>();
            try
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    FileList = new ObservableCollection<FileTreeNode> { BuildTree.BuildTreeFromFiles(allFiles, path, projectname) };
                });
            }
            catch
            {

                StatusMessage = "FileList View Failure";
            }

            StatusMessage = "FileList View Completed";
        }


        public FolderSelect FolderSelecter { get; }
        public TreeRefresh TreeRefreshHandler { get; }
        public iFolderService FolderService => _folderService;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

