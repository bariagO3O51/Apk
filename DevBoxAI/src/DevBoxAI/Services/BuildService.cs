using DevBoxAI.Core.Models;
using DevBoxAI.Core.Services;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace DevBoxAI.Services;

public class BuildService : IBuildService
{
    private readonly StringBuilder _buildLogs = new();

    public async Task<BuildResult> BuildProjectAsync(AndroidProject project, BuildConfiguration config)
    {
        var startTime = DateTime.Now;
        _buildLogs.Clear();

        try
        {
            // Check if project path exists
            if (!Directory.Exists(project.Path))
            {
                return new BuildResult
                {
                    Success = false,
                    Errors = new List<string> { "Project path does not exist" }
                };
            }

            // Find gradlew
            var gradlewPath = Path.Combine(project.Path, "gradlew.bat");
            if (!File.Exists(gradlewPath))
            {
                // Create gradle wrapper if not exists
                await CreateGradleWrapperAsync(project.Path);
            }

            // Build APK
            var buildType = config.BuildType == BuildType.Debug ? "assembleDebug" : "assembleRelease";
            var success = await RunGradleTaskAsync(project.Path, buildType);

            if (!success)
            {
                return new BuildResult
                {
                    Success = false,
                    Errors = new List<string> { "Gradle build failed. Check logs for details." },
                    BuildDuration = DateTime.Now - startTime
                };
            }

            // Find APK
            var apkPath = FindApk(project.Path, config.BuildType);

            return new BuildResult
            {
                Success = true,
                ApkPath = apkPath,
                BuildDuration = DateTime.Now - startTime
            };
        }
        catch (Exception ex)
        {
            return new BuildResult
            {
                Success = false,
                Errors = new List<string> { ex.Message },
                BuildDuration = DateTime.Now - startTime
            };
        }
    }

    public async Task<bool> InstallApkAsync(string apkPath, string? deviceId = null)
    {
        try
        {
            var adbPath = FindAdbPath();
            if (adbPath == null)
            {
                _buildLogs.AppendLine("ADB not found. Please install Android SDK.");
                return false;
            }

            var args = deviceId != null
                ? $"-s {deviceId} install -r \"{apkPath}\""
                : $"install -r \"{apkPath}\"";

            var result = await RunProcessAsync(adbPath, args);
            return result;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<string>> GetConnectedDevicesAsync()
    {
        var devices = new List<string>();

        try
        {
            var adbPath = FindAdbPath();
            if (adbPath == null)
                return devices;

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = "devices",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines.Skip(1)) // Skip "List of devices attached"
            {
                var parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && parts[1].Contains("device"))
                {
                    devices.Add(parts[0]);
                }
            }
        }
        catch
        {
            // Ignore errors
        }

        return devices;
    }

    public Task<string> GetBuildLogsAsync()
    {
        return Task.FromResult(_buildLogs.ToString());
    }

    private async Task<bool> RunGradleTaskAsync(string projectPath, string task)
    {
        var gradlewPath = Path.Combine(projectPath, "gradlew.bat");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = gradlewPath,
                Arguments = task,
                WorkingDirectory = projectPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                _buildLogs.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                _buildLogs.AppendLine($"ERROR: {e.Data}");
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return process.ExitCode == 0;
    }

    private async Task CreateGradleWrapperAsync(string projectPath)
    {
        // Create basic gradle wrapper files
        // This is a simplified version - in production, you'd download the actual wrapper

        var gradlewBat = @"@rem Gradle startup script for Windows
gradle %*";

        await File.WriteAllTextAsync(Path.Combine(projectPath, "gradlew.bat"), gradlewBat);
    }

    private string? FindApk(string projectPath, BuildType buildType)
    {
        var buildTypeStr = buildType == BuildType.Debug ? "debug" : "release";
        var apkDir = Path.Combine(projectPath, "app", "build", "outputs", "apk", buildTypeStr);

        if (!Directory.Exists(apkDir))
            return null;

        var apkFiles = Directory.GetFiles(apkDir, "*.apk");
        return apkFiles.FirstOrDefault();
    }

    private string? FindAdbPath()
    {
        // Try common ADB locations
        var commonPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Android", "Sdk", "platform-tools", "adb.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Android", "android-sdk", "platform-tools", "adb.exe"),
            "adb.exe" // Try PATH
        };

        foreach (var path in commonPaths)
        {
            if (File.Exists(path))
                return path;
        }

        return null;
    }

    private async Task<bool> RunProcessAsync(string filename, string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                _buildLogs.AppendLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        await process.WaitForExitAsync();

        return process.ExitCode == 0;
    }
}
