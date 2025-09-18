using CommunityToolkit.Mvvm.Messaging.Messages;
using MongoDB.Driver;
using MongoDB.Bson;


namespace CadEye_WebVersion.ViewModels.Messages
{
    public class SendTime : ValueChangedMessage<DateTime>
    {
        public SendTime(DateTime time) : base(time) { }
    }
}
