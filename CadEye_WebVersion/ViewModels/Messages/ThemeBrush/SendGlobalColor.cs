using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CadEye_WebVersion.ViewModels.Messages.ThemeBrush
{
    public class SendGlobalColor : ValueChangedMessage<System.Windows.Media.Brush>
    {
        public SendGlobalColor(System.Windows.Media.Brush color) : base(color) { }
    }

}
