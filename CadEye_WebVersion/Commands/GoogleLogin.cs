using CadEye_WebVersion.Models;
using CadEye_WebVersion.Services.Google;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.ViewModels.Messages;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Data;
using CadEye_WebVersion.Views;
using System.Diagnostics;

namespace CadEye_WebVersion.Commands
{
    public class GoogleLogin
    {
        public AsyncRelayCommand LoginCommand { get; }
        public readonly IGoogleService _googleService;
        private readonly IUserControlService _userControlService;

        public GoogleLogin(
            IGoogleService googleService,
            IUserControlService userControlService
            )
        {
            LoginCommand = new AsyncRelayCommand(OnLoginCheck);
            _googleService = googleService;
            _userControlService = userControlService;
        }

        public async Task OnLoginCheck()
        {
            await Task.Run(async () =>
            {
                (string GoogleId, string Email, string Name) = await _googleService.GoogleLogin();
                if (GoogleId == null || Email == null)
                {
                    MessageBox.Show("Login Failed, Please try again.");
                    return;
                }

                AppSettings.UserGoogleId = GoogleId;

                // Mongo User DB 불러오기
                var settings = MongoClientSettings.FromConnectionString(AppSettings.ServerIP);
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);
                var client = new MongoClient(settings);
                var dbnamelist = client.ListDatabaseNames().ToList();

                var dbfiltered = dbnamelist
                    .Where(name => name.Contains($"{GoogleId}_&_"))
                    .Select(name => name.Replace($"{GoogleId}_&_", ""))
                    .ToList();


                _userControlService.Init();

                bool theme = true;
                string _Email = Email;
                int CADThread = 1;
                int PDFThread = 1;
                List<UserEntry> ConnectUser = new List<UserEntry>();
                DateTime CreatedAt = DateTime.Now;
                DateTime LastLogin = DateTime.Now;
                string Role = "User";
                ObjectId id = new ObjectId();

                var call_node = await _userControlService.FindAsync(GoogleId);
                if (call_node != null)
                {
                    id = call_node.id;
                    theme = call_node.Theme ?? true;
                    Email = call_node.Email ?? "";
                    CADThread = call_node.CADThread;
                    PDFThread = call_node.PDFThread;
                    ConnectUser = call_node.ConnectUser;
                    CreatedAt = call_node.CreatedAt;
                }

                var admin_node = new AdminEntity()
                {
                    id = id,
                    Googleid = GoogleId,
                    Email = _Email,
                    Theme = theme,
                    CADThread = CADThread,
                    PDFThread = PDFThread,
                    ConnectUser = ConnectUser,
                    CreatedAt = CreatedAt,
                    LastLogin = LastLogin,
                    Role = Role,
                };

                
                if (call_node! != null)
                {
                    await _userControlService.UpdateAsync(admin_node);
                }
                else
                {
                    await _userControlService.AddAsync(admin_node);
                }

                AppSettings.ThemeToggle = theme;
                AppSettings.ZwcadThreads = admin_node.CADThread == 0 ? 4 : admin_node.CADThread;
                AppSettings.PDFTasks = admin_node.PDFThread == 0 ? 4 : admin_node.PDFThread;

                LoginSession.Admin = admin_node;
                LoginSession.GoogleId = GoogleId;
                LoginSession.Email = _Email;
                LoginSession.Databases = dbfiltered;
                LoginSession.UserName = Name;
                

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var loginWindow = System.Windows.Application.Current.Windows
                                        .OfType<System.Windows.Window>()
                                        .FirstOrDefault(w => w is LoginWindow);
                    if (loginWindow != null)
                        loginWindow.DialogResult = true; // 모달 종료
                });

            });
        }
    }
}
