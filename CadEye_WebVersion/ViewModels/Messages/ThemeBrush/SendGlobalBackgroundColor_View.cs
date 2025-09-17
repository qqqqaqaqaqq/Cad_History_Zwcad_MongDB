using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CadEye_WebVersion.ViewModels.Messages.ThemeBrush
{
    public class SendGlobalBackgroundColor_View : ValueChangedMessage<System.Windows.Media.Brush>
    {
        public SendGlobalBackgroundColor_View(System.Windows.Media.Brush color) : base(color) { }
    }

}
