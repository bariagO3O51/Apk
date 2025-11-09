namespace DevBoxAI.Core.Models;

public class GenerationRequest
{
    public string Prompt { get; set; } = string.Empty;
    public GenerationType Type { get; set; }
    public AndroidProject? ExistingProject { get; set; }
    public Dictionary<string, object>? Context { get; set; }
}

public enum GenerationType
{
    NewProject,
    AddScreen,
    AddFeature,
    ModifyUI,
    AddIntegration,
    RefactorCode,
    AddTests,
    FixBug
}

public class GenerationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<GeneratedFile> GeneratedFiles { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public AndroidProject? UpdatedProject { get; set; }
}

public class GeneratedFile
{
    public string Path { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public FileType Type { get; set; }
    public bool IsModified { get; set; }
}

public enum FileType
{
    Kotlin,
    XML,
    Gradle,
    Manifest,
    Resource,
    Asset,
    Test,
    Other
}
