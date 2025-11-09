using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevBoxAI.Core.Models;
using DevBoxAI.Core.Services;
using DevBoxAI.AndroidGenerator;
using System.Collections.ObjectModel;
using System.Windows;

namespace DevBoxAI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IProjectService _projectService;
    private readonly ICodeGenerationService _codeGenerationService;
    private readonly IBuildService _buildService;
    private readonly ProjectGenerator _projectGenerator;

    [ObservableProperty]
    private ObservableCollection<ChatMessage> _chatMessages = new();

    [ObservableProperty]
    private string _currentMessage = string.Empty;

    [ObservableProperty]
    private AndroidProject? _currentProject;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _statusIcon = "CheckCircle";

    [ObservableProperty]
    private bool _isProcessing = false;

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    [ObservableProperty]
    private bool _isDarkMode = false;

    [ObservableProperty]
    private string _selectedDeviceFrame = "Pixel 7";

    [ObservableProperty]
    private ObservableCollection<GeneratedFile> _generatedFiles = new();

    [ObservableProperty]
    private string _selectedFileContent = string.Empty;

    public ObservableCollection<string> DeviceFrames { get; } = new()
    {
        "Pixel 7", "Pixel 7 Pro", "Samsung S23", "Tablet 10\""
    };

    public MainViewModel(
        IProjectService projectService,
        ICodeGenerationService codeGenerationService,
        IBuildService buildService,
        ProjectGenerator projectGenerator)
    {
        _projectService = projectService;
        _codeGenerationService = codeGenerationService;
        _buildService = buildService;
        _projectGenerator = projectGenerator;

        // Add welcome message
        ChatMessages.Add(new ChatMessage
        {
            Role = ChatRole.Assistant,
            Content = "Willkommen bei DevBoxAI! Ich helfe dir, Android-Apps zu erstellen. " +
                     "Beschreibe mir deine App-Idee, und ich generiere den kompletten Code für dich.\n\n" +
                     "Beispiele:\n" +
                     "- 'Erstelle eine ToDo-App mit Login und Cloud-Sync'\n" +
                     "- 'Baue eine Fitness-Tracker-App mit Schrittzähler'\n" +
                     "- 'Generiere eine E-Commerce-App mit Warenkorb'"
        });
    }

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentMessage))
            return;

        var userMessage = new ChatMessage
        {
            Role = ChatRole.User,
            Content = CurrentMessage
        };

        ChatMessages.Add(userMessage);
        var prompt = CurrentMessage;
        CurrentMessage = string.Empty;

        IsProcessing = true;
        StatusMessage = "AI generiert Code...";

        try
        {
            // Determine generation type
            var generationType = DetermineGenerationType(prompt);

            var request = new GenerationRequest
            {
                Prompt = prompt,
                Type = generationType,
                ExistingProject = CurrentProject
            };

            if (generationType == GenerationType.NewProject && CurrentProject == null)
            {
                await CreateNewProjectFromPromptAsync(request);
            }
            else
            {
                await ModifyExistingProjectAsync(request);
            }

            StatusMessage = "Code erfolgreich generiert!";
            StatusIcon = "CheckCircle";
        }
        catch (Exception ex)
        {
            var errorMessage = new ChatMessage
            {
                Role = ChatRole.System,
                Content = $"Fehler: {ex.Message}"
            };
            ChatMessages.Add(errorMessage);

            StatusMessage = "Fehler bei der Code-Generierung";
            StatusIcon = "AlertCircle";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private async Task CreateNewProjectFromPromptAsync(GenerationRequest request)
    {
        // Extract project name and package from prompt
        var projectName = ExtractProjectName(request.Prompt);
        var packageName = $"com.devboxai.{projectName.ToLower().Replace(" ", "")}";

        // Create project
        CurrentProject = await _projectService.CreateProjectAsync(projectName, packageName);
        CurrentProject.Configuration.AppName = projectName;

        // Generate project structure
        var projectPath = await _projectGenerator.CreateProjectStructureAsync(CurrentProject);

        // Generate code using AI
        var result = await _codeGenerationService.GenerateFromPromptAsync(request);

        if (result.Success)
        {
            GeneratedFiles.Clear();
            foreach (var file in result.GeneratedFiles)
            {
                GeneratedFiles.Add(file);
            }

            CurrentProject = result.UpdatedProject ?? CurrentProject;

            var assistantMessage = new ChatMessage
            {
                Role = ChatRole.Assistant,
                Content = $"Projekt '{projectName}' wurde erfolgreich erstellt!\n\n" +
                         $"Generierte Dateien: {result.GeneratedFiles.Count}\n" +
                         $"Projektpfad: {projectPath}\n\n" +
                         "Du kannst jetzt:\n" +
                         "- Die Vorschau ansehen (Preview-Tab)\n" +
                         "- Den Code bearbeiten (Code-Tab)\n" +
                         "- Eine APK erstellen (Build APK Button)"
            };
            ChatMessages.Add(assistantMessage);
        }
    }

    private async Task ModifyExistingProjectAsync(GenerationRequest request)
    {
        if (CurrentProject == null)
        {
            var errorMsg = new ChatMessage
            {
                Role = ChatRole.System,
                Content = "Kein Projekt geladen. Bitte erstelle zuerst ein Projekt."
            };
            ChatMessages.Add(errorMsg);
            return;
        }

        var result = await _codeGenerationService.GenerateFromPromptAsync(request);

        if (result.Success)
        {
            foreach (var file in result.GeneratedFiles)
            {
                var existing = GeneratedFiles.FirstOrDefault(f => f.Path == file.Path);
                if (existing != null)
                {
                    GeneratedFiles.Remove(existing);
                }
                GeneratedFiles.Add(file);
            }

            CurrentProject = result.UpdatedProject ?? CurrentProject;

            var assistantMessage = new ChatMessage
            {
                Role = ChatRole.Assistant,
                Content = $"Änderungen erfolgreich angewendet!\n\n" +
                         $"Aktualisierte Dateien: {result.GeneratedFiles.Count}\n\n" +
                         result.Message
            };
            ChatMessages.Add(assistantMessage);
        }
    }

    private GenerationType DetermineGenerationType(string prompt)
    {
        var lowerPrompt = prompt.ToLower();

        if (lowerPrompt.Contains("erstelle") || lowerPrompt.Contains("neue app") ||
            lowerPrompt.Contains("generiere app"))
            return GenerationType.NewProject;

        if (lowerPrompt.Contains("screen") || lowerPrompt.Contains("seite") ||
            lowerPrompt.Contains("ansicht"))
            return GenerationType.AddScreen;

        if (lowerPrompt.Contains("feature") || lowerPrompt.Contains("funktion"))
            return GenerationType.AddFeature;

        if (lowerPrompt.Contains("ui") || lowerPrompt.Contains("design") ||
            lowerPrompt.Contains("layout"))
            return GenerationType.ModifyUI;

        if (lowerPrompt.Contains("integration") || lowerPrompt.Contains("api") ||
            lowerPrompt.Contains("firebase") || lowerPrompt.Contains("datenbank"))
            return GenerationType.AddIntegration;

        return GenerationType.AddFeature;
    }

    private string ExtractProjectName(string prompt)
    {
        // Simple extraction - in production, use AI to extract
        var keywords = new[] { "erstelle", "baue", "generiere", "app", "anwendung" };

        var words = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var relevantWords = words.Where(w => !keywords.Contains(w.ToLower())).ToList();

        if (relevantWords.Any())
        {
            return string.Join(" ", relevantWords.Take(3));
        }

        return "MyApp";
    }

    [RelayCommand]
    private async Task NewProjectAsync()
    {
        CurrentMessage = "Erstelle eine neue Android App";
        await SendMessageAsync();
    }

    [RelayCommand]
    private async Task OpenProjectAsync()
    {
        // TODO: Implement project opening dialog
        MessageBox.Show("Projekt öffnen wird implementiert...", "Info");
    }

    [RelayCommand]
    private void Settings()
    {
        // TODO: Implement settings dialog
        MessageBox.Show("Einstellungen werden implementiert...", "Info");
    }

    [RelayCommand]
    private void ShowPreview()
    {
        SelectedTabIndex = 0;
    }

    [RelayCommand]
    private void ShowCode()
    {
        SelectedTabIndex = 1;
    }

    [RelayCommand]
    private void ShowFiles()
    {
        SelectedTabIndex = 2;
    }

    [RelayCommand]
    private async Task BuildApkAsync()
    {
        if (CurrentProject == null)
        {
            MessageBox.Show("Kein Projekt geladen!", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        IsProcessing = true;
        StatusMessage = "APK wird erstellt...";

        try
        {
            var buildConfig = new BuildConfiguration
            {
                BuildType = BuildType.Debug,
                MinifyEnabled = false
            };

            var result = await _buildService.BuildProjectAsync(CurrentProject, buildConfig);

            if (result.Success)
            {
                var message = new ChatMessage
                {
                    Role = ChatRole.Assistant,
                    Content = $"APK erfolgreich erstellt!\n\n" +
                             $"Pfad: {result.ApkPath}\n" +
                             $"Build-Dauer: {result.BuildDuration.TotalSeconds:F1}s\n\n" +
                             "Die APK ist bereit zur Installation!"
                };
                ChatMessages.Add(message);

                MessageBox.Show($"APK erstellt: {result.ApkPath}", "Erfolg",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var errorMsg = string.Join("\n", result.Errors);
                MessageBox.Show($"Build fehlgeschlagen:\n{errorMsg}", "Fehler",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler beim Build: {ex.Message}", "Fehler",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsProcessing = false;
            StatusMessage = "Ready";
        }
    }
}
