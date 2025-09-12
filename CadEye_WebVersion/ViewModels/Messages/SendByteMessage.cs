using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Windows.Forms.Integration;

namespace CadEye_WebVersion.ViewModels.Messages
{
    public class SendByteMessage : ValueChangedMessage<byte[]>
    {
        public SendByteMessage(byte[] pdfbyte) : base(pdfbyte) { }
    }
}
