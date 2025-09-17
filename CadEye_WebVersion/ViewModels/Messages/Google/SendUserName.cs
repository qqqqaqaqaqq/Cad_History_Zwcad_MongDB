using CommunityToolkit.Mvvm.Messaging.Messages;


namespace CadEye_WebVersion.ViewModels.Messages
{
    public class SendUserName : ValueChangedMessage<string>
    {
        public SendUserName(string username) : base(username) { }
    }
}
