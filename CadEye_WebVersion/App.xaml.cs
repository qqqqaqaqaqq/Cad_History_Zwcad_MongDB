using CadEye_WebVersion.Services.WindowService;
using CadEye_WebVersion.Commands;
using CadEye_WebVersion.Commands.Helpers;
using CadEye_WebVersion.Services.FileCheck;
using CadEye_WebVersion.Services.FileSystem;
using CadEye_WebVersion.Services.FileWatcher.ProjectFolder;
using CadEye_WebVersion.Services.FileWatcher.Repository;
using CadEye_WebVersion.Services.FileWatcher.RepositoryPdf;
using CadEye_WebVersion.Services.FolderService;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.Services.Mongo.Services;
using CadEye_WebVersion.Services.PDF;
using CadEye_WebVersion.Services.Zwcad;
using CadEye_WebVersion.ViewModels;
using CadEye_WebVersion.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

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

            // -----------------
            // 헬퍼/커맨드 등록
            // -----------------
            services.AddTransient<AsyncCommand>();
            services.AddTransient(typeof(AsyncCommandT<>));
            services.AddTransient<RelayCommand>();

            // =====================
            // 기타 서비스
            // =====================
            services.AddSingleton<iFolderService, FolderService>();
            services.AddSingleton<IFileCheckService, FileCheckService>();
            services.AddSingleton<IFileSystem, FileSystem>();
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
            services.AddSingleton<InformationView>();
        }
    }
    public static class AppSettings
    {
        public static string? DatabaseType { get; set; }
        public static string? RepositoryPdfFolder { get; set; }
    }
}
