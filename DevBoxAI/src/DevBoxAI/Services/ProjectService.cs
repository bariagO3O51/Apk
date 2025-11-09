using DevBoxAI.Core.Models;
using DevBoxAI.Core.Services;
using System.IO;
using System.Text.Json;

namespace DevBoxAI.Services;

public class ProjectService : IProjectService
{
    private readonly string _projectsDirectory;
    private readonly string _recentProjectsFile;

    public ProjectService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _projectsDirectory = Path.Combine(appData, "DevBoxAI", "Projects");
        _recentProjectsFile = Path.Combine(appData, "DevBoxAI", "recent_projects.json");

        Directory.CreateDirectory(_projectsDirectory);
    }

    public async Task<AndroidProject> CreateProjectAsync(string name, string packageName)
    {
        var project = new AndroidProject
        {
            Name = name,
            PackageName = packageName,
            Path = Path.Combine(_projectsDirectory, name),
            Configuration = new AppConfiguration
            {
                AppName = name
            }
        };

        await SaveProjectAsync(project);
        await AddToRecentProjectsAsync(project);

        return project;
    }

    public async Task<AndroidProject?> LoadProjectAsync(string path)
    {
        try
        {
            var projectFile = Path.Combine(path, "devboxai_project.json");
            if (!File.Exists(projectFile))
                return null;

            var json = await File.ReadAllTextAsync(projectFile);
            var project = JsonSerializer.Deserialize<AndroidProject>(json);

            if (project != null)
            {
                await AddToRecentProjectsAsync(project);
            }

            return project;
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveProjectAsync(AndroidProject project)
    {
        Directory.CreateDirectory(project.Path);

        var projectFile = Path.Combine(project.Path, "devboxai_project.json");
        var json = JsonSerializer.Serialize(project, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(projectFile, json);
    }

    public async Task<List<AndroidProject>> GetRecentProjectsAsync()
    {
        try
        {
            if (!File.Exists(_recentProjectsFile))
                return new List<AndroidProject>();

            var json = await File.ReadAllTextAsync(_recentProjectsFile);
            var projects = JsonSerializer.Deserialize<List<AndroidProject>>(json);

            return projects ?? new List<AndroidProject>();
        }
        catch
        {
            return new List<AndroidProject>();
        }
    }

    public async Task DeleteProjectAsync(string projectId)
    {
        var projects = await GetRecentProjectsAsync();
        var project = projects.FirstOrDefault(p => p.Id == projectId);

        if (project != null)
        {
            // Remove from recent projects
            projects.Remove(project);
            await SaveRecentProjectsAsync(projects);

            // Delete project directory
            if (Directory.Exists(project.Path))
            {
                Directory.Delete(project.Path, true);
            }
        }
    }

    private async Task AddToRecentProjectsAsync(AndroidProject project)
    {
        var projects = await GetRecentProjectsAsync();

        // Remove if already exists
        projects.RemoveAll(p => p.Id == project.Id);

        // Add to front
        projects.Insert(0, project);

        // Keep only last 10
        if (projects.Count > 10)
        {
            projects = projects.Take(10).ToList();
        }

        await SaveRecentProjectsAsync(projects);
    }

    private async Task SaveRecentProjectsAsync(List<AndroidProject> projects)
    {
        var json = JsonSerializer.Serialize(projects, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(_recentProjectsFile, json);
    }
}
