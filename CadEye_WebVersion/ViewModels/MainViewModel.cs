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
using MongoDB.Driver;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

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
            IProjectFolderWatcherService projectFolderWatcherService,
            IProjectPath projectPath)
        {
            #region Commnand
            AddNaviCommand = new RelayCommand(AddNavi);
            SwitchViewRadioCommand = new RelayCommand<object?>(OnHostOpen);
            CloseHostCommand = new RelayCommand<object?>(OnHostClose);
            #endregion

            #region Init Properties
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
            _projectPath = projectPath;
            #endregion

            #region Message
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
            WeakReferenceMessenger.Default.Register<SendGlobalBackgroundColor>(this, async (r, m) =>
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    GlobalBackgroundColor = m.Value;
                });
            });
            WeakReferenceMessenger.Default.Register<SendGlobalBackgroundColor_View>(this, async (r, m) =>
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    GlobalBackgroundColor_View = m.Value;
                });
            });
            WeakReferenceMessenger.Default.Register<SendGlobalBorderBrush>(this, async (r, m) =>
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    GlobalBorderBrush = m.Value;
                });
            });
            WeakReferenceMessenger.Default.Register<SendForeground>(this, async (r, m) =>
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    GlobalForeground = m.Value;
                });
            });
            WeakReferenceMessenger.Default.Register<SendUserName>(this, (r, m) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!string.IsNullOrEmpty(m.Value))
                    {
                        LoginWindowVisibility = Visibility.Collapsed;
                        StatusMessage = "Login Success";
                        UserName = $"{m.Value}님! 반갑습니다.";
                    }
                });
            });
            WeakReferenceMessenger.Default.Register<SendGoogleId>(this, (r, m) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!string.IsNullOrEmpty(m.Value))
                    {
                        GoogleId = m.Value;
                        StatusMessage = "Connect Success";
                    }
                });
            });
            WeakReferenceMessenger.Default.Register<SendUsersDatabase>(this, (r, m) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (m.Value.Count() > 0)
                    {
                        UserDataBase = new ObservableCollection<string>(m.Value);
                        StatusMessage = "Connect Success";
                    }
                });
            });
            WeakReferenceMessenger.Default.Register<SendPageName>(this, async (r, m) =>
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var homeView = NaviTapHosts.FirstOrDefault(x => x.id == pagenumber);
                    if (homeView == null)
                        return;
                    homeView.PageName = m.Value;
                });
            });
            #endregion
        }

        #region GoogleId
        private string _googleId = string.Empty;
        public string GoogleId
        {
            get => _googleId;
            set
            {
                _googleId = value;
                OnPropertyChanged();
                AppSettings.UserGoogleId = GoogleId;
            }
        }
        #endregion 

        #region Fields & Properties
        private readonly iFolderService _folderService;
        public readonly IChildFileService _childFileService;
        public readonly IEventEntryService _eventEntryService;
        public readonly IImageEntryService _imageEntryService;
        public readonly IRefEntryService _refEntryService;
        public readonly IProjectFolderWatcherService _projectFolderWatcherService;
        public readonly IRepositoryPdfWatcherService _pdfWatcherService;
        public readonly IRepositoryWatcherService _dwgWatcherService;
        public readonly IPdfService _pdfService;
        public readonly IProjectPath _projectPath;
        public FileSystemWatcher? _watcher_repository_project;
        public FileSystemWatcher? _watcher_repository_dwg;
        public FileSystemWatcher? _watcher_repository_pdf;
        public FolderSelect FolderSelecter { get; }
        public TreeRefresh TreeRefreshHandler { get; }
        public iFolderService FolderService => _folderService;
        #endregion

        #region LoginWindow
        private Visibility _loginWindowVisibility = Visibility.Visible;
        public Visibility LoginWindowVisibility
        {
            get => _loginWindowVisibility;
            set
            {
                _loginWindowVisibility = value;
                OnPropertyChanged();
            }
        }
        public object _loginHostContent = App.ServiceProvider!.GetRequiredService<LoginPageView>();
        public object LoginHostContent
        {
            get => _loginHostContent;
            set { _loginHostContent = value; OnPropertyChanged(); }
        }
        #endregion

        #region FirstPage
        private string _username = "Guest";
        public string UserName
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Navi
        public ObservableCollection<NaviTapHost> _naviTapHosts = new ObservableCollection<NaviTapHost>();
        public ObservableCollection<NaviTapHost> NaviTapHosts
        {
            get => _naviTapHosts;
            set
            {
                _naviTapHosts = value;
                OnPropertyChanged();
            }
        }

        private object _homehostpage = App.ServiceProvider!.GetRequiredService<HomeView>();
        public object HomePageHost
        {
            get => _homehostpage;
            set
            {
                _homehostpage = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand AddNaviCommand { get; }
        public void AddNavi()
        {

            for (int i = 1; i < navistatus.Length; i++)
            {
                if (!navistatus[i])
                {
                    foreach (var host in NaviTapHosts)
                    {
                        host.IsChecked = false;
                    }

                    var homeView = new NaviTapHost();
                    homeView.id = i;
                    homeView.PageName = "Home";
                    homeView.Page = App.ServiceProvider!.GetRequiredService<HomeView>();
                    homeView.IsChecked = true;
                    homeView.PageVisibility = Visibility.Visible;

                    pagenumber = i;

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        NaviTapHosts.Add(homeView);
                        HostPage = homeView!.Page!;
                        UserNameVisibility = Visibility.Collapsed;
                    });

                    navistatus[i] = !navistatus[i];
                    return;
                }

                if (NaviTapHosts.Count == navistatus.Length - 2)
                {
                    AddNaviVisibility = Visibility.Collapsed;
                }
            }
        }
        private int pagenumber = 0;

        public bool[] navistatus = new bool[] { false, false, false, false, false, false, false };
        public bool[] navicheck = new bool[] { false, false, false, false, false, false, false };

        public RelayCommand<object?> SwitchViewRadioCommand { get; }

        private object _hostpage = new object();
        public object HostPage
        {
            get => _hostpage;
            set
            {
                _hostpage = value;
                OnPropertyChanged();
            }
        }
        public void OnHostOpen(object? id)
        {
            if (id == null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    UserNameVisibility = Visibility.Visible;
                });
                return;
            }
            int idx = int.Parse(id.ToString()!);
            var selected = NaviTapHosts.FirstOrDefault(x => x.id == idx);

            pagenumber = idx;

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                HostPage = selected!.Page!;
                UserNameVisibility = Visibility.Collapsed;
            });
        }

        private Visibility _addNaviVisibility = Visibility.Visible;
        public Visibility AddNaviVisibility
        {
            get => _addNaviVisibility;
            set
            {
                _addNaviVisibility = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<object?> CloseHostCommand { get; }
        public void OnHostClose(object? id)
        {
            if (id == null)
                return;
            int idx = int.Parse(id.ToString()!);
            var selected = NaviTapHosts.FirstOrDefault(x => x.id == idx);
            if (selected != null)
            {
                navistatus[idx] = false;
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    NaviTapHosts.Remove(selected);
                    HostPage = null!;
                    UserNameVisibility = Visibility.Visible;
                });
            }
            AddNaviVisibility = Visibility.Visible;
        }

        private Visibility _userNameVisibility = Visibility.Visible;
        public Visibility UserNameVisibility
        {
            get => _userNameVisibility;
            set
            {
                _userNameVisibility = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region StatusMessage
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
        private async Task SatatusUpdate(string status)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                StatusMessage = status;
            });
        }
        #endregion

        #region GlobalColor
        private System.Windows.Media.Brush _globalBackgroundColor = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#80EEEEEE"));
        private System.Windows.Media.Brush _globalForground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#000"));
        private System.Windows.Media.Brush _globalBackgoundColor_View = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#fff"));
        private System.Windows.Media.Brush _globalBorderBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFCCCCCC"));
        private System.Windows.Media.FontFamily _globalFontFamily = new System.Windows.Media.FontFamily("Segoe UI");
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
        public System.Windows.Media.Brush GlobalBackgroundColor_View
        {
            get => _globalBackgoundColor_View;
            set
            {
                if (_globalBackgoundColor_View != value)
                {
                    _globalBackgoundColor_View = value;
                    OnPropertyChanged();
                }
            }
        }
        public System.Windows.Media.Brush GlobalForeground
        {
            get => _globalForground;
            set
            {
                if (_globalForground != value)
                {
                    _globalForground = value;
                    OnPropertyChanged();
                }
            }
        }
        public System.Windows.Media.Brush GlobalBackgroundColor
        {
            get => _globalBackgroundColor;
            set
            {
                _globalBackgroundColor = value;
                OnPropertyChanged();
            }
        }
        public System.Windows.Media.FontFamily GlobalFontFamily
        {
            get => _globalFontFamily;
            set
            {
                _globalFontFamily = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region ProjectName
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
        #endregion

        #region TreeView
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
        public async void SetTreeVeiw(string projectname)
        {
            // Mongo User DB 불러오기
            var settings = MongoClientSettings.FromConnectionString(AppSettings.ServerIP);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            var dbnamelist = client.ListDatabaseNames().ToList();

     
            var dbfiltered = dbnamelist.Find(x => x.Equals($"{GoogleId}_&_{projectname}"));
        

            if (dbfiltered == null) return;
            string dbName = dbfiltered.Trim().Replace(" ", "_").Replace(".", "_");

            StatusMessage = "Project Name Completed";

            var database = client.GetDatabase(dbName);

            _childFileService.Init(dbName);
            _eventEntryService.Init(dbName);
            _imageEntryService.Init(dbName);
            _refEntryService.Init(dbName);
            _projectPath.Init(dbName);

            var allFiles = await _childFileService.FindAllAsync() ?? new List<ChildFile>();
            var node = await _projectPath.NameFindAsync(projectname);

            // TreeView 작성
            try
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    FileList = new ObservableCollection<FileTreeNode> { BuildTree.BuildTreeFromFiles(allFiles, node.ProjectFullName, projectname) };
                });
            }
            catch
            {

                StatusMessage = "Tree View Failure";
            }

            StatusMessage = "Tree View Completed";
        }
        #endregion

        #region FolderIcon
        private ImageSource _folderIcon = SystemIconProvider.FolderIcon();
        public ImageSource FolderIcon
        {
            get => _folderIcon;
        }
        #endregion

        #region UserDataBaseList
        private ObservableCollection<string> _userDataBase = new ObservableCollection<string>();
        public ObservableCollection<string> UserDataBase
        {
            get => _userDataBase;
            set
            {
                _userDataBase = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Combobox
        private string? _comboSelectedItem;
        public string? ComboSelectedItem
        {
            get => _comboSelectedItem;
            set
            {
                _comboSelectedItem = value;
                if (ComboSelectedItem != null)
                {
                    SetTreeVeiw(ComboSelectedItem);
                }
            }
        }
        #endregion

        #region MainEvent
        private string Folder_path = string.Empty;
        public async void OnFolderSelected(string path)
        {
            StatusMessage = "Start...";

            // 프로젝트 네임 셋팅
            string projectname = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(projectname))
                return;

            Folder_path = path;

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

            // Mongo collection 셋팅
            var settings = MongoClientSettings.FromConnectionString(AppSettings.ServerIP);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);

            dbName = $"{AppSettings.UserGoogleId}_&_{dbName}";
            if (dbName.Length > 63)
            {
                System.Windows.MessageBox.Show("프로젝트 명이 너무 깁니다. 폴더명을 줄여주세요.", "알림");
                return;
            }
            StatusMessage = "Project Name Completed";
            var database = client.GetDatabase(dbName);

            _childFileService.Init(dbName);
            _eventEntryService.Init(dbName);
            _imageEntryService.Init(dbName);
            _refEntryService.Init(dbName);
            _projectPath.Init(dbName);

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
                    await _projectPath.AddAsync(new ProjectPath
                    {
                        ProjectFullName = path,
                    });
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
        }
        #endregion


        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

