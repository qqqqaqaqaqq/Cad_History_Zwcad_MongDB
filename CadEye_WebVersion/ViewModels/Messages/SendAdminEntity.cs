using CadEye_WebVersion.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Windows.Forms.Integration;

namespace CadEye_WebVersion.ViewModels.Messages
{
    public class SendAdminEntity : ValueChangedMessage<AdminEntity>
    {
        public SendAdminEntity(AdminEntity entity) : base(entity) { }
    }
}
