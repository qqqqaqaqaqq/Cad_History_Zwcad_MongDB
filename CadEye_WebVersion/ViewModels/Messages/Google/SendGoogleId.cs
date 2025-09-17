using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadEye_WebVersion.ViewModels.Messages
{
    public class SendGoogleId : ValueChangedMessage<string>
    {
        public SendGoogleId(string username) : base(username) { }
    }
}
