using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CadEye_WebVersion.ViewModels.Messages.ThemeBrush
{
    public class SendForeGroundBrush : ValueChangedMessage<System.Windows.Media.Brush>
    {
        public SendForeGroundBrush(System.Windows.Media.Brush color) : base(color) { }
    }

}
