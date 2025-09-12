using CommunityToolkit.Mvvm.Messaging.Messages;
using MongoDB.Driver;
using MongoDB.Bson;


namespace CadEye_WebVersion.ViewModels.Messages
{
    public class SelectedTreeNodeMessage : ValueChangedMessage<ObjectId>
    {
        public SelectedTreeNodeMessage(ObjectId id) : base(id) { }
    }
}
