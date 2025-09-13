using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CadEye_WebVersion.ViewModels.Messages.ThemeBrush
{
    public class SendViewContainBackGround : ValueChangedMessage<System.Windows.Media.Brush>
    {
        public SendViewContainBackGround(System.Windows.Media.Brush color) : base(color) { }
    }

}
