using CadEye_WebVersion.Commands;
using CadEye_WebVersion.Infrastructure.Utils;
using CadEye_WebVersion.Models;
using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Services.FileWatcher.ProjectFolder;
using CadEye_WebVersion.Services.FileWatcher.Repository;
using CadEye_WebVersion.Services.FileWatcher.RepositoryPdf;
using CadEye_WebVersion.Services.FolderService;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.Services.PDF;
using CadEye_WebVersion.ViewModels.Messages;
using CadEye_WebVersion.ViewModels.Messages.ThemeBrush;
using CadEye_WebVersion.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using MongoDB.Driver;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CadEye_WebVersion.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel(iFolderService folderService,
            IChildFileService childFileService,
            IEventEntryService eventEntryService,
            IImageEntryService imageEntryService,
            IRefEntryService refEntryService,
            IPdfService pdfService,
            IRepositoryPdfWatcherService fileWatcherService,
            IRepositoryWatcherService dwgWatcherService,
            IProjectFolderWatcherService projectFolderWatcherService)
        {
            AddRadioCommand = new RelayCommand(OnAddRadio);
            CloseHomeViewCommand = new RelayCommand<object?>(OnCloseHomeView);
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
            WeakReferenceMessenger.Default.Register<SendPageName>(this, async (r, m) =>
            {
                Pagename = m.Value;
            });
        }

        private readonly iFolderService _folderService;
        public readonly IChildFileService _childFileService;
        public readonly IEventEntryService _eventEntryService;
        public readonly IImageEntryService _imageEntryService;
        public readonly IRefEntryService _refEntryService;
        public readonly IProjectFolderWatcherService _projectFolderWatcherService;
        public readonly IRepositoryPdfWatcherService _pdfWatcherService;
        public readonly IRepositoryWatcherService _dwgWatcherService;
        public readonly IPdfService _pdfService;
        public FileSystemWatcher? _watcher_repository_project;
        public FileSystemWatcher? _watcher_repository_dwg;
        public FileSystemWatcher? _watcher_repository_pdf;



        // Navi
        // 하드코딩으로 제작됨 => 사유 아이디어 부족.
        public RelayCommand AddRadioCommand { get; }
        public RelayCommand<object?> CloseHomeViewCommand { get; }
        private Visibility _homeViewBtn1Visibility = Visibility.Collapsed;
        private Visibility _homeViewBtn2Visibility = Visibility.Collapsed;
        private Visibility _homeViewBtn3Visibility = Visibility.Collapsed;
        private Visibility _homeViewBtn4Visibility = Visibility.Collapsed;
        private Visibility _homeViewBtn5Visibility = Visibility.Collapsed;
        private Visibility _addBtnVisibility = Visibility.Visible;
        public Visibility AddBtnVisibility
        {
            get => _addBtnVisibility;
            set
            {
                _addBtnVisibility = value;
                OnPropertyChanged();
            }
        }
        public Visibility HomeViewBtn1Visibility
        {
            get => _homeViewBtn1Visibility;
            set
            {
                _homeViewBtn1Visibility = value;
                OnPropertyChanged();
            }
        }
        public Visibility HomeViewBtn2Visibility
        {
            get => _homeViewBtn2Visibility;
            set
            {
                _homeViewBtn2Visibility = value;
                OnPropertyChanged();
            }
        }
        public Visibility HomeViewBtn3Visibility
        {
            get => _homeViewBtn3Visibility;
            set
            {
                _homeViewBtn3Visibility = value;
                OnPropertyChanged();
            }
        }
        public Visibility HomeViewBtn4Visibility
        {
            get => _homeViewBtn4Visibility;
            set
            {
                _homeViewBtn4Visibility = value;
                OnPropertyChanged();
            }
        }
        public Visibility HomeViewBtn5Visibility
        {
            get => _homeViewBtn5Visibility;
            set
            {
                _homeViewBtn5Visibility = value;
                OnPropertyChanged();
            }
        }
        private void OnAddRadio()
        {
            int newIndex = 0;

            if (HomeViewBtn1Visibility == Visibility.Collapsed)
            {
                HomeViewBtn1Visibility = Visibility.Visible;
                newIndex = 1;
            }
            else if (HomeViewBtn2Visibility == Visibility.Collapsed)
            {
                HomeViewBtn2Visibility = Visibility.Visible;
                newIndex = 2;
            }
            else if (HomeViewBtn3Visibility == Visibility.Collapsed)
            {
                HomeViewBtn3Visibility = Visibility.Visible;
                newIndex = 3;
            }
            else if (HomeViewBtn4Visibility == Visibility.Collapsed)
            {
                HomeViewBtn4Visibility = Visibility.Visible;
                newIndex = 4;
            }
            else if (HomeViewBtn5Visibility == Visibility.Collapsed)
            {
                HomeViewBtn5Visibility = Visibility.Visible;
                AddBtnVisibility = Visibility.Collapsed;
                newIndex = 5;
            }

            if (newIndex > 0)
            {
                // Dispatcher로 UI 갱신 후 체크!
                System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    IndexCheck = newIndex;
                });
            }
        }
        private int _indexcheck = 0;
        public int IndexCheck
        {
            get => _indexcheck;
            set
            {
                _indexcheck = value;
                UpdateChecks();
            }
        }
        private void UpdateChecks()
        {
            HomeCheck1 = IndexCheck == 1;
            HomeCheck2 = IndexCheck == 2;
            HomeCheck3 = IndexCheck == 3;
            HomeCheck4 = IndexCheck == 4;
            HomeCheck5 = IndexCheck == 5;
        }
        private bool _homeCheck1;
        public bool HomeCheck1 
        { get => _homeCheck1; 
            set { _homeCheck1 = value; OnPropertyChanged(); } }
        private bool _homeCheck2; 
        public bool HomeCheck2 
        { get => _homeCheck2; 
            set { _homeCheck2 = value; OnPropertyChanged(); } }
        private bool _homeCheck3;
        public bool HomeCheck3 
        { get => _homeCheck3; 
            set { _homeCheck3 = value; OnPropertyChanged(); } }
        private bool _homeCheck4; 
        public bool HomeCheck4
        { get => _homeCheck4; set 
            { _homeCheck4 = value; OnPropertyChanged(); } }
        private bool _homeCheck5; 
        public bool HomeCheck5 
        { get => _homeCheck5; 
            set { _homeCheck5 = value; OnPropertyChanged(); } }
        public void OnCloseHomeView(object? parameter)
        {
            if (parameter == null) return;

            int index = System.Convert.ToInt32(parameter);
            switch (index)
            {
                case 1:
                    NaviName1 = "Home";
                    HomeCheck1 = false;
                    HomeHost1Content = App.ServiceProvider.GetRequiredService<HomeView>();
                    HomeViewBtn1Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    NaviName2 = "Home";
                    HomeCheck2 = false;
                    HomeHost2Content = App.ServiceProvider.GetRequiredService<HomeView>();
                    HomeViewBtn2Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    NaviName3 = "Home";
                    HomeCheck3 = false;
                    HomeHost3Content = App.ServiceProvider.GetRequiredService<HomeView>();
                    HomeViewBtn3Visibility = Visibility.Collapsed;
                    break;
                case 4:
                    NaviName4 = "Home";
                    HomeCheck4 = false;
                    HomeHost4Content = App.ServiceProvider.GetRequiredService<HomeView>();
                    HomeViewBtn4Visibility = Visibility.Collapsed;
                    break;
                case 5:
                    NaviName5 = "Home";
                    HomeCheck5 = false;
                    HomeHost5Content = App.ServiceProvider.GetRequiredService<HomeView>();
                    HomeViewBtn5Visibility = Visibility.Collapsed;
                    break;
            }

            AddBtnVisibility = Visibility.Visible;

            IndexCheck = 0;
        }
        public string _naviname1 = "Home";
        public string NaviName1
        {
            get => _naviname1;
            set
            {
                _naviname1 = value;
                OnPropertyChanged();
            }
        }
        public string _naviname2 = "Home";
        public string NaviName2
        {
            get => _naviname2;
            set
            {
                _naviname2 = value;
                OnPropertyChanged();
            }
        }
        public string _naviname3 = "Home";
        public string NaviName3
        {
            get => _naviname3;
            set
            {
                _naviname3 = value;
                OnPropertyChanged();
            }
        }
        public string _naviname4 = "Home";
        public string NaviName4
        {
            get => _naviname4;
            set
            {
                _naviname4 = value;
                OnPropertyChanged();
            }
        }
        public string _naviname5 = "Home";
        public string NaviName5
        {
            get => _naviname5;
            set
            {
                _naviname5 = value;
                OnPropertyChanged();
            }
        }
        public string ActivePage
        {
            get
            {
                if (HomeCheck1) return NaviName1;
                if (HomeCheck2) return NaviName2;
                if (HomeCheck3) return NaviName3;
                if (HomeCheck4) return NaviName4;
                if (HomeCheck5) return NaviName5;
                return null; // 활성화된 페이지 없음
            }
        }
        public string _pagename = "Home";
        public string Pagename
        {
            get => _pagename;
            set
            {
                _pagename = value;
                RedirectHandler(Pagename);
            }
        }
        public void RedirectHandler(string pagename)
        {
            if (HomeCheck1)
                NaviName1 = pagename;
            else if (HomeCheck2)
                NaviName2 = pagename;
            else if (HomeCheck3)
                NaviName3 = pagename;
            else if (HomeCheck4)
                NaviName4 = pagename;
            else if (HomeCheck5)
                NaviName5 = pagename;
        }
        private object _homeHost1Content = App.ServiceProvider.GetRequiredService<HomeView>();
        public object HomeHost1Content
        {
            get => _homeHost1Content;
            set { _homeHost1Content = value; OnPropertyChanged(); }
        }
        private object _homeHost2Content = App.ServiceProvider.GetRequiredService<HomeView>();
        public object HomeHost2Content
        {
            get => _homeHost2Content;
            set { _homeHost2Content = value; OnPropertyChanged(); }
        }
        private object _homeHost3Content = App.ServiceProvider.GetRequiredService<HomeView>();
        public object HomeHost3Content
        {
            get => _homeHost3Content;
            set { _homeHost3Content = value; OnPropertyChanged(); }
        }
        private object _homeHost4Content = App.ServiceProvider.GetRequiredService<HomeView>();
        public object HomeHost4Content
        {
            get => _homeHost4Content;
            set { _homeHost4Content = value; OnPropertyChanged(); }
        }
        private object _homeHost5Content = App.ServiceProvider.GetRequiredService<HomeView>();
        public object HomeHost5Content
        {
            get => _homeHost5Content;
            set { _homeHost5Content = value; OnPropertyChanged(); }
        }


        // 상태메세지
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



        // 다크모드, 화이트 모드
        private System.Windows.Media.Brush _globalcolor = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#80EEEEEE"));
        private System.Windows.Media.Brush _theme = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#000"));
        private System.Windows.Media.Brush _viewCotainBackGround = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#fff"));
        private System.Windows.Media.Brush _globalBorderBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFCCCCCC"));
        public System.Windows.Media.Brush GlobalBorderBrush
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
        public System.Windows.Media.Brush ViewCotainBackGround
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
        public System.Windows.Media.Brush Theme
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



        // 프로젝트 네임
        private string _projectname = "Project Name";
        public string ProjectName
        {
            get => _projectname;
            set
            {
                _projectname = value;
                OnPropertyChanged();
            }
        }



        // 트리
        private ObservableCollection<Models.FileTreeNode> _fileList = new ObservableCollection<FileTreeNode>();
        public ObservableCollection<FileTreeNode> FileList
        {
            get => _fileList;
            set
            {
                _fileList = value;
                OnPropertyChanged();
            }
        }
        private FileTreeNode _selectedTreeNode = new FileTreeNode();
        public FileTreeNode SelectedTreeNode
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



        // 폴더 아이콘
        private ImageSource _folderIcon = SystemIconProvider.FolderIcon();
        public ImageSource FolderIcon
        {
            get => _folderIcon;
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

