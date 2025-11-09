namespace DevBoxAI.Core.Models;

public class AndroidProject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime ModifiedAt { get; set; } = DateTime.Now;

    public AppConfiguration Configuration { get; set; } = new();
    public BuildConfiguration BuildConfig { get; set; } = new();
    public List<Screen> Screens { get; set; } = new();
    public List<DataModel> DataModels { get; set; } = new();
    public List<Integration> Integrations { get; set; } = new();
    public GitRepository? GitRepo { get; set; }
}

public class AppConfiguration
{
    public string AppName { get; set; } = string.Empty;
    public string MinSdkVersion { get; set; } = "24";
    public string TargetSdkVersion { get; set; } = "34";
    public string CompileSdkVersion { get; set; } = "34";
    public string VersionCode { get; set; } = "1";
    public string VersionName { get; set; } = "1.0.0";
    public bool UseMaterial3 { get; set; } = true;
    public bool UseDarkTheme { get; set; } = true;
    public StateManagementType StateManagement { get; set; } = StateManagementType.MVVM;
    public DependencyInjectionType DependencyInjection { get; set; } = DependencyInjectionType.Hilt;
}

public enum StateManagementType
{
    MVVM,
    MVI
}

public enum DependencyInjectionType
{
    Hilt,
    Koin,
    Manual
}

public class BuildConfiguration
{
    public bool MinifyEnabled { get; set; } = true;
    public bool ProGuardEnabled { get; set; } = true;
    public BuildType BuildType { get; set; } = BuildType.Debug;
    public string? KeystorePath { get; set; }
    public string? KeystorePassword { get; set; }
    public string? KeyAlias { get; set; }
    public string? KeyPassword { get; set; }
}

public enum BuildType
{
    Debug,
    Release
}

public class Screen
{
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<UIComponent> Components { get; set; } = new();
    public ScreenType Type { get; set; } = ScreenType.Standard;
}

public enum ScreenType
{
    Standard,
    Form,
    List,
    Detail,
    Settings,
    Onboarding
}

public class UIComponent
{
    public string Name { get; set; } = string.Empty;
    public ComponentType Type { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

public enum ComponentType
{
    Button,
    TextField,
    Text,
    Image,
    Card,
    List,
    AppBar,
    BottomNav,
    Drawer,
    Custom
}

public class DataModel
{
    public string Name { get; set; } = string.Empty;
    public List<Field> Fields { get; set; } = new();
}

public class Field
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsNullable { get; set; }
}

public class Integration
{
    public string Name { get; set; } = string.Empty;
    public IntegrationType Type { get; set; }
    public Dictionary<string, string> Configuration { get; set; } = new();
}

public enum IntegrationType
{
    Firebase,
    Supabase,
    REST,
    GraphQL,
    Stripe,
    Maps,
    Analytics,
    Crashlytics
}

public class GitRepository
{
    public string Url { get; set; } = string.Empty;
    public string Branch { get; set; } = "main";
    public string? LastCommit { get; set; }
}
