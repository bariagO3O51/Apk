using DevBoxAI.Core.Models;

namespace DevBoxAI.Core.Services;

public interface IProjectService
{
    Task<AndroidProject> CreateProjectAsync(string name, string packageName);
    Task<AndroidProject?> LoadProjectAsync(string path);
    Task SaveProjectAsync(AndroidProject project);
    Task<List<AndroidProject>> GetRecentProjectsAsync();
    Task DeleteProjectAsync(string projectId);
}
