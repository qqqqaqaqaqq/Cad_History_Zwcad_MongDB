using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CadEye_WebVersion.ViewModels.Messages.ThemeBrush
{
    public class SendGlobalBorderBrush : ValueChangedMessage<System.Windows.Media.Brush>
    {
        public SendGlobalBorderBrush(System.Windows.Media.Brush color) : base(color) { }
    }

}
