using CadEye_WebVersion.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CadEye_WebVersion.ViewModels.Messages
{
    public class SendFileTreeNode : ValueChangedMessage<FileTreeNode>
    {
        public SendFileTreeNode(FileTreeNode status) : base(status) { }
    }
}