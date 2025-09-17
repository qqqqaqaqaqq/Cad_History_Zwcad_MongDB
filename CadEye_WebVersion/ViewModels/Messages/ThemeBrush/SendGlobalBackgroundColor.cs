using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CadEye_WebVersion.ViewModels.Messages.ThemeBrush
{
    public class SendGlobalBackgroundColor : ValueChangedMessage<System.Windows.Media.Brush>
    {
        public SendGlobalBackgroundColor(System.Windows.Media.Brush color) : base(color) { }
    }

}
