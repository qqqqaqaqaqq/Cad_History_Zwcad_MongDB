using CommunityToolkit.Mvvm.Messaging.Messages;
using MongoDB.Driver;
using MongoDB.Bson;


namespace CadEye_WebVersion.ViewModels.Messages
{
    public class SendObjectId : ValueChangedMessage<ObjectId>
    {
        public SendObjectId(ObjectId id) : base(id) { }
    }
}
