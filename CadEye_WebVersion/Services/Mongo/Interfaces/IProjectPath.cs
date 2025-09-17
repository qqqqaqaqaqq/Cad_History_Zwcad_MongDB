using CadEye_WebVersion.Models.Entity;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CadEye_WebVersion.Services.Mongo.Interfaces
{
    public interface IProjectPath
    {
        void Init(string path);
        Task AddAsync(ProjectPath project);
        Task<ProjectPath> NameFindAsync(string projectname);
    }
}
