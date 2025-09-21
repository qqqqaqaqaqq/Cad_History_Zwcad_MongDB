using CadEye_WebVersion.Commands;
using CadEye_WebVersion.Services.Mongo.Interfaces;
using CadEye_WebVersion.Services.Google;

namespace CadEye_WebVersion.ViewModels
{
    public class LoginWindowModel
    {
        public GoogleLogin LoginEvent { get; }
        public readonly IGoogleService _googleService;
        public readonly IUserControlService _userControlService;

        public LoginWindowModel(
            IGoogleService googleService,
            IUserControlService userControlService)
        {
            _googleService = googleService;
            _userControlService = userControlService;
            LoginEvent = new GoogleLogin(_googleService, _userControlService);
        }
    }
}
