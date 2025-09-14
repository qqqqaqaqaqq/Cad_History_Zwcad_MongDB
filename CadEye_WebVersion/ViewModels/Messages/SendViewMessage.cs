using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CadEye_WebVersion.ViewModels.Messages.ThemeBrush
{
    public class SendViewMessage : ValueChangedMessage<object>
    {
        public SendViewMessage(object view) : base(view) { }
    }

}
