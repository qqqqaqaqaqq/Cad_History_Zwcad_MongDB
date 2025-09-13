using CadEye_WebVersion.Models.Entity;
using CadEye_WebVersion.Models;


namespace CadEye_WebVersion.Infrastructure.Utils
{
    public static class BuildTree
    {
        public static FileTreeNode BuildTreeFromFiles(List<ChildFile> files, string projectRootPath, string projectname)
        {
            var root = new CadEye_WebVersion.Models.FileTreeNode
            {
                Name = projectname,
            };

            foreach (var file in files)
            {
                string relativePath = System.IO.Path.GetRelativePath(projectRootPath, file.File_FullName);

                var parts = relativePath.Split(
                    new[] {
                    System.IO.Path.DirectorySeparatorChar,
                    System.IO.Path.AltDirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries);

                var current = root;

                foreach (var part in parts)
                {
                    var child = current.Children.FirstOrDefault(c => c.Name == part);
                    if (child == null)
                    {
                        child = new CadEye_WebVersion.Models.FileTreeNode
                        {
                            Name = part
                        };

                        if (part == parts.Last())
                        {
                            child.Id = file.Id;
                        }
                        current.Children.Add(child);
                    }
                    current = child;
                }
            }

            return root;
        }
    }
}
