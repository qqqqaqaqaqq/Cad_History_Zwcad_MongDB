using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CadEye_WebVersion.ViewModels.Messages.ThemeBrush
{
    public class SendForeground : ValueChangedMessage<System.Windows.Media.Brush>
    {
        public SendForeground(System.Windows.Media.Brush color) : base(color) { }
    }

}
