using CadEye_WebVersion.Services.WindowService;
using CadEye_WebVersion.Commands;
using CadEye_WebVersion.Services.FileWatcher.ProjectFolder;
using CadEye_WebVersion.Services.FileWatcher.Repository;
using CadEye_WebVersion.Services.FileWatcher.RepositoryPdf;
using CadEye_WebVersion.Services.FolderService;
using CadEye_WebVersion.Services.Mongo.Services;
using CadEye_WebVersion.Services.PDF;
using CadEye_WebVersion.Services.Zwcad;
using CadEye_WebVersion.ViewModels;
using CadEye_WebVersion.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using CadEye_WebVersion.Services.Mongo.Interfaces;

namespace CadEye_WebVersion
{
    public partial class App : System.Windows.Application   
    {
        public IServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppSettings.DatabaseType = "LocalMongo";
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            AppSettings.ThemeToggle = true; // 이후 MongoDb에서 가져오기
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
            services.AddSingleton<HomeView>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<SettingViewModel>();
            services.AddSingleton<InformationViewModel>();

            // =====================
            // Commands
            // =====================
            services.AddSingleton<ViewSwitch>();

            // =====================
            // View
            // =====================
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<SettingView>();
            services.AddSingleton<InformationView>();
        }
    }
    public static class AppSettings
    {
        public static bool ThemeToggle { get; set; }
        public static string? ProjectPath { get; set; }
        public static string? DatabaseType { get; set; }
        public static string? RepositoryPdfFolder { get; set; }
        public static string? RepositoryDwgFolder { get; set; }
    }
}
