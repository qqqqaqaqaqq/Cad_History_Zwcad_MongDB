using MongoDB.Bson;

namespace CadEye_WebVersion.Models
{
    public class UserEntry
    {
        public ObjectId id { get; set; }
        public string? name { get; set; }
        public string? role { get; set; }
        public bool isActive { get; set; }
    }
    public class AdminEntity
    {
        public AdminEntity()
        {
            ConnectUser = new List<UserEntry>();
        }

        public ObjectId id { get; set; }
        public string? Googleid { get; set; }
        public string? Email { get; set; }
        public bool? Theme { get; set; }
        public int CADThread { get; set; }
        public int PDFThread { get; set; }
        public List<UserEntry> ConnectUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLogin { get; set; }
        public string Role { get; set; } = "Admin";
    }
}
