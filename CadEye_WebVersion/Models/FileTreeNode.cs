using System.Collections.ObjectModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CadEye_WebVersion.Models
{
    public class FileTreeNode
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string? Name { get; set; }
        public ObservableCollection<FileTreeNode> Children { get; set; } = new ObservableCollection<FileTreeNode>();
    }
}
