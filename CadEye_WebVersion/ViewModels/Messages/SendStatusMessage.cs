using CommunityToolkit.Mvvm.Messaging.Messages;
using MongoDB.Driver;
using MongoDB.Bson;


namespace CadEye_WebVersion.ViewModels.Messages
{
    public class SendStatusMessage : ValueChangedMessage<string>
    {
        public SendStatusMessage(string status) : base(status) { }
    }
}
