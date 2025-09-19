using CadEye_WebVersion.Commands;
using CadEye_WebVersion.Infrastructure.Utils;
using CadEye_WebVersion.Services.FileWatcher.ProjectFolder;
using CadEye_WebVersion.Services.FileWatcher.Repository;
using CadEye_WebVersion.Services.FileWatcher.RepositoryPdf;
using CadEye_WebVersion.Services.FolderService;
using CadEye_WebVersion.Services.Google;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.Services.Mongo.Services;
using CadEye_WebVersion.Services.PDF;
using CadEye_WebVersion.Services.WindowService;
using CadEye_WebVersion.Services.Zwcad;
using CadEye_WebVersion.ViewModels;
using CadEye_WebVersion.ViewModels.Messages.SplashMessage;
using CadEye_WebVersion.Views;
using CommunityToolkit.Mvvm.Messaging;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace CadEye_WebVersion
{
    public partial class App : System.Windows.Application
    {
        #region field
        public static IServiceProvider? ServiceProvider { get; private set; }

        public readonly ThemeToggle themeToggle = new ThemeToggle();
        private Window? splashWindow;
        private System.Windows.Threading.Dispatcher? splashDispatcher;
        private Window? EnvWindow;
        #endregion

        #region Main
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string envPath = System.IO.Path.Combine(basePath, ".env");
            bool ReadFile = await RetryProvider.RetryAsync(() => Task.FromResult(System.IO.File.Exists(envPath)), 100, 100);
            if (!ReadFile)
            {
                EnvWindow = new EnvCreateWindow();
                EnvWindow.ShowDialog();
            }

            await ReadEnv();

            var thread = new Thread(() =>
            {
                splashDispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
                splashWindow = new SplashWindow();
                splashWindow.Show();
                System.Windows.Threading.Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            await InitializeAsync();

            if (ServiceProvider != null)
            {
                var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                AppSettings.ThemeToggle = "DarkMode";
                themeToggle.OnThemeToggle(AppSettings.ThemeToggle);

                WeakReferenceMessenger.Default.Send(new SplashMessage($"All Completed"));
                mainWindow.Show(); 
                await ShowClosed();
            }
        }
        #endregion

        #region InitializeAsync
        private async Task ShowClosed()
        {
            if (splashDispatcher != null && splashWindow != null)
            {
                await splashDispatcher.InvokeAsync(() =>
                {
                    splashWindow.Close();
                    splashDispatcher.InvokeShutdown();
                });
            }
        }

        private async Task ReadEnv()
        {
            await Task.Run(() =>
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string envPath = System.IO.Path.Combine(basePath, ".env");

                Env.Load(envPath);
                string connectionString = Env.GetString("MONGO_URI");
                string mygoogleId = Env.GetString("GOOGLE_ID");
                string mygoogleSecrete = Env.GetString("GOOGLE_SECRETE");

                if (string.IsNullOrEmpty(mygoogleId) || string.IsNullOrEmpty(mygoogleSecrete))
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        var result = System.Windows.MessageBox.Show(
                            "Env 로드 실패",
                            "알림",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );

                        if (result == MessageBoxResult.OK)
                        {
                            System.Windows.Application.Current.Shutdown();
                        }
                    });

                    return;
                }
                AppSettings.ServerIP = connectionString;
                AppSettings.MyGoogleId = mygoogleId;
                AppSettings.MyGoogleSecrete = mygoogleSecrete;
            });
        }

        private async Task InitializeAsync()
        {
            AppSettings.ZwcadThreads = 6; // 안전을 위해 기존 대비 2개추가 thread는 4개만돌아가도록
            AppSettings.PDFTasks = 6;
            await Task.Run(() =>
            {
                var services = new ServiceCollection();
                ConfigureServices(services);
                ServiceProvider = services.BuildServiceProvider();
            });
        }


        private void ConfigureServices(IServiceCollection services)
        {
            #region Service
            // =====================
            // MongoDB 관련 서비스
            // =====================

            services.AddSingleton<IChildFileService, MongoDBChildFile>();
            services.AddSingleton<IEventEntryService, MongoDBEventEntry>();
            services.AddSingleton<IImageEntryService, MongoDBImageEntry>();
            services.AddSingleton<IRefEntryService, MongoDBRefEntry>();
            services.AddSingleton<IUserControlService, UserControlService>();
            services.AddSingleton<IProjectPath, MongoDBProjectPath>();

            // =====================
            // 기타 서비스
            // =====================
            services.AddSingleton<iFolderService, FolderService>();
            services.AddSingleton<IWindowsService, WindowService>();

            // =====================
            // FileWatcher 서비스
            // =====================
            services.AddSingleton<IProjectFolderWatcherService, ProjectFolderWatcherService>();
            services.AddSingleton<IRepositoryWatcherService, RepositoryWatcherService>();
            services.AddSingleton<IRepositoryPdfWatcherService, RepositoryPdfWatcherService>();

            // =====================
            // Google 서비스
            // =====================
            services.AddSingleton<IGoogleService, GoogleService>();

            // =====================
            // PDF 서비스
            // =====================
            services.AddSingleton<IPdfService, PdfService>();

            // =====================
            // ZwCad 서비스
            // =====================
            services.AddSingleton<IZwcadService, ZwcadService>();

            // =====================
            // ViewModel
            // =====================
            services.AddSingleton<MainViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<SettingViewModel>();
            services.AddTransient<InformationViewModel>();
            services.AddTransient<LoginPageViewModel>();
            services.AddTransient<AdminViewModel>();

            // =====================
            // View
            // =====================
            services.AddSingleton<MainWindow>();
            services.AddTransient<HomeView>();
            services.AddTransient<SettingView>();
            services.AddTransient<InformationView>();
            services.AddTransient<LoginPageView>();
            services.AddTransient<AdminView>();
            #endregion

            #region Message
            WeakReferenceMessenger.Default.Send(new SplashMessage($"Service Loading..."));
            #endregion
        }

        #endregion
    }

    public static class AppSettings
    {
        public static int ZwcadThreads { get; set; }
        public static int PDFTasks { get; set; }
        public static string? MyGoogleId { get; set; }
        public static string? MyGoogleSecrete { get; set; }
        public static string? UserGoogleId { get; set; }
        public static string? ServerIP { get; set; }
        public static string? ThemeToggle { get; set; }
        public static string? ProjectPath { get; set; }
        public static string? DatabaseType { get; set; }
        public static string? RepositoryPdfFolder { get; set; }
        public static string? RepositoryDwgFolder { get; set; }
        public static string? ProjectName { get; set; }
    }
}
