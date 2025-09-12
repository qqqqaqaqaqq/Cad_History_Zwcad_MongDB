using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Windows.Forms.Integration;

namespace CadEye_WebVersion.ViewModels.Messages
{
    public class SendCompareByteMessage : ValueChangedMessage<byte[]>
    {
        public SendCompareByteMessage(byte[] pdfbyte) : base(pdfbyte) { }
    }
}
