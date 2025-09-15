using CadEye_WebVersion.Commands;
using CadEye_WebVersion.Services.FileWatcher.ProjectFolder;
using CadEye_WebVersion.Services.FileWatcher.Repository;
using CadEye_WebVersion.Services.FileWatcher.RepositoryPdf;
using CadEye_WebVersion.Services.FolderService;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.Services.Mongo.Services;
using CadEye_WebVersion.Services.PDF;
using CadEye_WebVersion.Services.WindowService;
using CadEye_WebVersion.Services.Zwcad;
using CadEye_WebVersion.ViewModels;
using CadEye_WebVersion.Views;
using CadEye_WebVersion.Services.Google;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using DotNetEnv;

namespace CadEye_WebVersion
{
    public partial class App : System.Windows.Application   
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        public readonly ThemeToggle themeToggle = new ThemeToggle();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Env.Load(@"C:\Users\Hong\Desktop\CadEye_WebVersion\CadEye_WebVersion\.env");
            string connectionString = Env.GetString("MONGO_URI");
            string mygoogleId = Env.GetString("GOOGLE_ID");
            string mygoogleSecrete = Env.GetString("GOOGLE_SECRETE");

            AppSettings.ServerIP = connectionString;
            AppSettings.MyGoogleId = mygoogleId;
            AppSettings.MyGoogleSecrete = mygoogleSecrete;
            AppSettings.DatabaseType = "LocalMongo";
            AppSettings.ProjectName = "CadEye";

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            AppSettings.ThemeToggle = "DarkMode";
            themeToggle.OnThemeToggle(AppSettings.ThemeToggle);
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // =====================
            // MongoDB 관련 서비스
            // =====================
            services.AddSingleton<IChildFileService, MongoDBChildFile>();
            services.AddSingleton<IEventEntryService, MongoDBEventEntry>();
            services.AddSingleton<IImageEntryService, MongoDBImageEntry>();
            services.AddSingleton<IRefEntryService, MongoDBRefEntry>();
            services.AddSingleton<IUserControlService, UserControlService>();

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
            // Commands
            // =====================
            services.AddSingleton<ViewSwitch>();
            
            // =====================
            // ViewModel
            // =====================
            services.AddSingleton<MainViewModel>();
            services.AddTransient<HomeViewModel>();       
            services.AddTransient<SettingViewModel>();      
            services.AddTransient<InformationViewModel>();
            services.AddTransient<LoginPageViewModel>();

            // =====================
            // View
            // =====================
            services.AddSingleton<MainWindow>();
            services.AddTransient<HomeView>();  
            services.AddTransient<SettingView>();
            services.AddTransient<InformationView>();
            services.AddTransient<LoginPageView>();
        }
    }
    public static class AppSettings
    {
        public static string? MyGoogleId { get; set; }
        public static string? MyGoogleSecrete { get; set; }
        public static string? ServerIP { get; set; }
        public static string? ThemeToggle { get; set; }
        public static string? ProjectPath { get; set; }
        public static string? DatabaseType { get; set; }
        public static string? RepositoryPdfFolder { get; set; }
        public static string? RepositoryDwgFolder { get; set; }
        public static string? ProjectName { get; set; }
    }
}
