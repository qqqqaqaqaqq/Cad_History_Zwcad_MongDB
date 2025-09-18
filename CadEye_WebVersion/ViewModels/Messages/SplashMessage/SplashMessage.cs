using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CadEye_WebVersion.ViewModels.Messages.SplashMessage
{
    public class SplashMessage : ValueChangedMessage<string>
    {
        public SplashMessage(string message) : base(message) { }
    }

}
