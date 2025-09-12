using System.Collections.ObjectModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CadEye_WebVersion.Models
{
    public class TreeNode
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string? Name { get; set; }
        public ObservableCollection<TreeNode> Children { get; set; } = new ObservableCollection<TreeNode>();
    }
}
