using DevBoxAI.Core.Models;

namespace DevBoxAI.Core.Services;

public interface IBuildService
{
    Task<BuildResult> BuildProjectAsync(AndroidProject project, BuildConfiguration config);
    Task<bool> InstallApkAsync(string apkPath, string? deviceId = null);
    Task<List<string>> GetConnectedDevicesAsync();
    Task<string> GetBuildLogsAsync();
}

public class BuildResult
{
    public bool Success { get; set; }
    public string? ApkPath { get; set; }
    public string? AabPath { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public TimeSpan BuildDuration { get; set; }
}
