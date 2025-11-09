using DevBoxAI.Core.Models;
using DevBoxAI.Core.Services;
using System.Text;
using System.Text.Json;

namespace DevBoxAI.AI.Services;

public class ClaudeCodeGenerationService : ICodeGenerationService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public ClaudeCodeGenerationService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.anthropic.com/v1/")
        };
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<GenerationResult> GenerateFromPromptAsync(GenerationRequest request)
    {
        try
        {
            var systemPrompt = BuildSystemPrompt(request);
            var userPrompt = BuildUserPrompt(request);

            var response = await CallClaudeAsync(systemPrompt, userPrompt);
            var result = ParseGenerationResponse(response, request);

            return result;
        }
        catch (Exception ex)
        {
            return new GenerationResult
            {
                Success = false,
                Message = $"Error generating code: {ex.Message}",
                Errors = new List<string> { ex.ToString() }
            };
        }
    }

    public async Task<string> GenerateScreenCodeAsync(Screen screen, AndroidProject project)
    {
        var prompt = $@"Generate a Kotlin Activity class for the following screen:
Name: {screen.Name}
Type: {screen.Type}
Description: {screen.Description}
Components: {JsonSerializer.Serialize(screen.Components)}

Use {project.Configuration.StateManagement} pattern with {project.Configuration.DependencyInjection} for dependency injection.
Use Material 3 design components.";

        var response = await CallClaudeAsync(
            "You are an expert Android developer specializing in Kotlin and Jetpack Compose.",
            prompt
        );

        return ExtractCodeFromResponse(response);
    }

    public async Task<string> GenerateViewModelCodeAsync(Screen screen, AndroidProject project)
    {
        var prompt = $@"Generate a ViewModel class for the screen '{screen.Name}' using {project.Configuration.StateManagement} pattern.
Include state management, business logic, and proper lifecycle handling.";

        var response = await CallClaudeAsync(
            "You are an expert Android developer specializing in MVVM/MVI architecture.",
            prompt
        );

        return ExtractCodeFromResponse(response);
    }

    public async Task<string> GenerateLayoutXmlAsync(Screen screen)
    {
        var prompt = $@"Generate an Android XML layout for:
Screen: {screen.Name}
Type: {screen.Type}
Components: {JsonSerializer.Serialize(screen.Components)}

Use Material 3 design, ConstraintLayout, and follow Android best practices.";

        var response = await CallClaudeAsync(
            "You are an expert Android UI developer.",
            prompt
        );

        return ExtractCodeFromResponse(response);
    }

    public async Task<string> GenerateGradleFileAsync(AndroidProject project)
    {
        var prompt = $@"Generate a build.gradle.kts (app level) file for:
App: {project.Name}
Package: {project.PackageName}
MinSDK: {project.Configuration.MinSdkVersion}
TargetSDK: {project.Configuration.TargetSdkVersion}
Integrations: {JsonSerializer.Serialize(project.Integrations.Select(i => i.Type))}

Include necessary dependencies for: Jetpack Compose, {project.Configuration.DependencyInjection}, Room, Retrofit, etc.";

        var response = await CallClaudeAsync(
            "You are an expert in Android Gradle configuration.",
            prompt
        );

        return ExtractCodeFromResponse(response);
    }

    public async Task<string> GenerateManifestAsync(AndroidProject project)
    {
        var prompt = $@"Generate AndroidManifest.xml for:
App: {project.Name}
Package: {project.PackageName}
Screens: {JsonSerializer.Serialize(project.Screens.Select(s => s.Name))}
Permissions needed: Based on integrations like {JsonSerializer.Serialize(project.Integrations)}";

        var response = await CallClaudeAsync(
            "You are an expert in Android manifest configuration.",
            prompt
        );

        return ExtractCodeFromResponse(response);
    }

    private async Task<string> CallClaudeAsync(string systemPrompt, string userPrompt)
    {
        var requestBody = new
        {
            model = "claude-sonnet-4-5-20250929",
            max_tokens = 8000,
            system = systemPrompt,
            messages = new[]
            {
                new { role = "user", content = userPrompt }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("messages", content);
        response.EnsureSuccessStatusCode();

        var responseText = await response.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseText);

        return responseObj.GetProperty("content")[0].GetProperty("text").GetString() ?? "";
    }

    private string BuildSystemPrompt(GenerationRequest request)
    {
        return @"You are DevBoxAI, an expert Android application architect and developer.
You generate production-ready, well-structured Android applications using:
- Kotlin as the primary language
- Jetpack Compose for modern UI
- Material 3 design system
- MVVM or MVI architecture
- Hilt or Koin for dependency injection
- Room for local database
- Retrofit/OkHttp for networking
- Comprehensive unit and instrumentation tests

Always follow Android best practices, SOLID principles, and write clean, maintainable code.
Generate complete, working code without placeholders or TODOs.";
    }

    private string BuildUserPrompt(GenerationRequest request)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Generation Type: {request.Type}");
        sb.AppendLine($"User Request: {request.Prompt}");

        if (request.ExistingProject != null)
        {
            sb.AppendLine($"\nExisting Project Context:");
            sb.AppendLine($"Name: {request.ExistingProject.Name}");
            sb.AppendLine($"Package: {request.ExistingProject.PackageName}");
            sb.AppendLine($"Screens: {JsonSerializer.Serialize(request.ExistingProject.Screens.Select(s => s.Name))}");
        }

        sb.AppendLine("\nPlease generate the complete code with proper structure and organization.");

        return sb.ToString();
    }

    private GenerationResult ParseGenerationResponse(string response, GenerationRequest request)
    {
        // Parse the Claude response and extract generated files
        // This is a simplified version - in production, you'd have more sophisticated parsing

        var files = new List<GeneratedFile>();

        // Extract code blocks from response
        var codeBlocks = ExtractCodeBlocks(response);

        foreach (var block in codeBlocks)
        {
            files.Add(new GeneratedFile
            {
                Path = block.path,
                Content = block.code,
                Type = DetermineFileType(block.path)
            });
        }

        return new GenerationResult
        {
            Success = true,
            Message = "Code generated successfully",
            GeneratedFiles = files
        };
    }

    private List<(string path, string code)> ExtractCodeBlocks(string response)
    {
        var blocks = new List<(string path, string code)>();

        // Simple regex to find code blocks with file paths
        // Format: ```kotlin:path/to/file.kt
        var lines = response.Split('\n');
        string? currentPath = null;
        var currentCode = new StringBuilder();
        bool inCodeBlock = false;

        foreach (var line in lines)
        {
            if (line.StartsWith("```"))
            {
                if (!inCodeBlock)
                {
                    // Starting a code block
                    inCodeBlock = true;
                    var parts = line.Substring(3).Split(':');
                    if (parts.Length > 1)
                    {
                        currentPath = parts[1].Trim();
                    }
                }
                else
                {
                    // Ending a code block
                    inCodeBlock = false;
                    if (currentPath != null)
                    {
                        blocks.Add((currentPath, currentCode.ToString()));
                    }
                    currentPath = null;
                    currentCode.Clear();
                }
            }
            else if (inCodeBlock)
            {
                currentCode.AppendLine(line);
            }
        }

        return blocks;
    }

    private string ExtractCodeFromResponse(string response)
    {
        var blocks = ExtractCodeBlocks(response);
        return blocks.FirstOrDefault().code ?? response;
    }

    private FileType DetermineFileType(string path)
    {
        var extension = Path.GetExtension(path).ToLower();
        return extension switch
        {
            ".kt" => FileType.Kotlin,
            ".xml" => FileType.XML,
            ".gradle" or ".kts" => FileType.Gradle,
            _ => FileType.Other
        };
    }
}
