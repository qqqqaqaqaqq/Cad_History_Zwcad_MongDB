using CommunityToolkit.Mvvm.Messaging.Messages;

namespace CadEye_WebVersion.ViewModels.Messages.ThemeBrush
{
    public class SendPageName : ValueChangedMessage<string>
    {
        public SendPageName(string name) : base(name) { }
    }

}
