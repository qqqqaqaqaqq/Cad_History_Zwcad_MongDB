using CadEye_WebVersion.Commands;
using CadEye_WebVersion.Infrastructure.Utils;
using CadEye_WebVersion.Models;
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
using System.Diagnostics;
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

            MessageBoxResult result1 = System.Windows.MessageBox.Show(
                "ZWCAD가 모두 꺼집니다.\n저장 완료하신 후 '예' 버튼을 눌러주세요.",
                "종료 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result1 == MessageBoxResult.Yes)
            {
                await MainRun();
            }
            else
            {
                System.Windows.Application.Current.Shutdown();
            }
        }
        #endregion

        private async Task MainRun()
        {
            foreach (Process proc in Process.GetProcessesByName("ZWCAD"))
            {
                proc.Kill();
                proc.WaitForExit();
            }

            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string envPath = System.IO.Path.Combine(basePath, ".env");
            bool ReadFile = await RetryProvider.RetryAsync(() => Task.FromResult(System.IO.File.Exists(envPath)), 100, 100);
            if (!ReadFile)
            {
                EnvWindow = new EnvCreateWindow();
                EnvWindow.ShowDialog();
            }

            await ReadEnv();


            var services = new ServiceCollection();
            await Task.Run(() =>
            {
                ConfigureServices(services);
            });

            LoginWindow loginwindow = new LoginWindow();
            bool? result = loginwindow.ShowDialog();
            if (result == true)
            {

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

                await Task.Run(() =>
                {
                    ExServices(services);
                });

                ServiceProvider = services.BuildServiceProvider();

                var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                this.MainWindow = mainWindow;
                this.ShutdownMode = ShutdownMode.OnMainWindowClose;

                themeToggle.OnThemeToggle(AppSettings.ThemeToggle);

                WeakReferenceMessenger.Default.Send(new SplashMessage($"All Completed"));
                Thread.Sleep(1000);
                await ShowClosed();
                mainWindow.Show();
            }
        }

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


        private void ExServices(IServiceCollection services)
        {
            // =====================
            // ZwCad 서비스
            // =====================
            services.AddSingleton<IZwcadService, ZwcadService>();

            // =====================
            // FileWatcher 서비스
            // =====================
            services.AddSingleton<IProjectFolderWatcherService, ProjectFolderWatcherService>();
            services.AddSingleton<IRepositoryWatcherService, RepositoryWatcherService>();
            services.AddSingleton<IRepositoryPdfWatcherService, RepositoryPdfWatcherService>();
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
            // Google 서비스
            // =====================
            services.AddSingleton<IGoogleService, GoogleService>();

            // =====================
            // PDF 서비스
            // =====================
            services.AddSingleton<IPdfService, PdfService>();

            // =====================
            // ViewModel
            // =====================
            services.AddSingleton<MainViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<SettingViewModel>();
            services.AddTransient<InformationViewModel>();
            services.AddTransient<LoginWindowModel>();
            services.AddSingleton<AdminViewModel>();

            // =====================
            // View
            // =====================
            services.AddSingleton<MainWindow>();
            services.AddTransient<HomeView>();
            services.AddTransient<SettingView>();
            services.AddTransient<InformationView>();
            services.AddSingleton<AdminView>();
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
        public static bool? ThemeToggle { get; set; }
        public static string? ProjectPath { get; set; }
        public static string? DatabaseType { get; set; }
        public static string? RepositoryPdfFolder { get; set; }
        public static string? RepositoryDwgFolder { get; set; }
        public static string? ProjectName { get; set; }
    }

    public static class LoginSession
    {
        public static AdminEntity? Admin { get; set; }
        public static string? Email { get; set; }
        public static string? GoogleId { get; set; }
        public static List<string>? Databases { get; set; }
        public static string? UserName { get; set; }
    }
}
