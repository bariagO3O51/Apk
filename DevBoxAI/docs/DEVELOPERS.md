# DevBoxAI - Entwickler-Dokumentation

## Architektur-Übersicht

DevBoxAI ist eine modulare Windows-Desktop-Anwendung, die aus mehreren Komponenten besteht:

### Projekt-Struktur

```
DevBoxAI/
├── src/
│   ├── DevBoxAI/                  # Main WPF Application
│   ├── DevBoxAI.Core/             # Shared Models & Interfaces
│   ├── DevBoxAI.AI/               # AI Integration (Claude)
│   └── DevBoxAI.AndroidGenerator/ # Android Code Generation
```

## Module im Detail

### 1. DevBoxAI (WPF App)

**Technologien:**
- WPF (Windows Presentation Foundation)
- Material Design in XAML
- MVVM Pattern mit CommunityToolkit.Mvvm
- Dependency Injection mit Microsoft.Extensions.DependencyInjection

**Hauptkomponenten:**

- **MainWindow.xaml**: Haupt-UI mit Chat, Preview, Code-Editor
- **MainViewModel.cs**: Business Logic für UI
- **Services/**: ProjectService, BuildService
- **Converters/**: XAML Value Converters

### 2. DevBoxAI.Core

**Zweck:** Shared Models und Service-Interfaces

**Modelle:**
- `AndroidProject`: Projekt-Konfiguration
- `ChatMessage`: Chat-Nachrichten
- `GenerationRequest/Result`: AI-Generierungs-Requests
- `BuildConfiguration`: Build-Einstellungen
- `Screen, UIComponent`: UI-Definitionen

**Interfaces:**
- `IProjectService`: Projekt-Verwaltung
- `ICodeGenerationService`: Code-Generierung
- `IBuildService`: Build & Deploy

### 3. DevBoxAI.AI

**Technologien:**
- Anthropic Claude API
- HttpClient für API-Kommunikation
- System.Text.Json für Serialisierung

**Hauptklassen:**

- `ClaudeCodeGenerationService`: AI-Integration
  - Prompt Engineering für verschiedene Generierungs-Typen
  - Response Parsing (Code-Block-Extraktion)
  - Context-Building für existierende Projekte

**Prompt-Struktur:**

```
System Prompt: Expert Android Developer Persona
User Prompt:
  - Generation Type (NewProject, AddScreen, etc.)
  - User Request (natürliche Sprache)
  - Existing Project Context (optional)
```

### 4. DevBoxAI.AndroidGenerator

**Zweck:** Android-Projekt-Generierung

**Hauptklassen:**

- `ProjectGenerator`: Erstellt Projekt-Struktur
  - Directory-Layout (Java-Package-Struktur)
  - Gradle-Dateien (settings.gradle.kts, build.gradle.kts)
  - AndroidManifest.xml
  - Base Activities & Resources

**Generierte Projekt-Struktur:**

```
app/
├── src/
│   ├── main/
│   │   ├── java/com/package/
│   │   │   ├── ui/        # Compose UI
│   │   │   ├── data/      # Repository, Room, Retrofit
│   │   │   ├── domain/    # UseCases, Models
│   │   │   └── di/        # Hilt Modules
│   │   ├── res/
│   │   │   ├── values/
│   │   │   ├── drawable/
│   │   │   └── mipmap/
│   │   └── AndroidManifest.xml
│   ├── test/              # Unit Tests
│   └── androidTest/       # UI Tests
├── build.gradle.kts
└── proguard-rules.pro
```

## Datenfluss

### Neue Projekt-Erstellung

```
User Input (Chat)
    ↓
MainViewModel.SendMessageAsync()
    ↓
ClaudeCodeGenerationService.GenerateFromPromptAsync()
    ↓
Claude API Call → Response
    ↓
Parse Response → GenerationResult
    ↓
ProjectGenerator.CreateProjectStructureAsync()
    ↓
File System Write (Kotlin, XML, Gradle)
    ↓
Update UI (Preview, File Tree)
```

### Build-Prozess

```
User Click "Build APK"
    ↓
MainViewModel.BuildApkAsync()
    ↓
BuildService.BuildProjectAsync()
    ↓
Gradle Wrapper Execution (gradlew assembleDebug/Release)
    ↓
Monitor Build Output
    ↓
Find Generated APK
    ↓
Return BuildResult
```

## AI-Integration Details

### Claude API Configuration

**Model:** `claude-sonnet-4-5-20250929`

**API Endpoint:** `https://api.anthropic.com/v1/messages`

**Headers:**
- `x-api-key`: API Key
- `anthropic-version`: `2023-06-01`
- `Content-Type`: `application/json`

**Request Body:**

```json
{
  "model": "claude-sonnet-4-5-20250929",
  "max_tokens": 8000,
  "system": "System Prompt...",
  "messages": [
    {
      "role": "user",
      "content": "User Prompt..."
    }
  ]
}
```

### Prompt Engineering

**System Prompt Pattern:**

```
You are DevBoxAI, an expert Android application architect.
You generate production-ready Android applications using:
- Kotlin
- Jetpack Compose & Material 3
- MVVM/MVI Architecture
- Hilt Dependency Injection
- Room Database
- Retrofit Networking
- Comprehensive Tests

Always follow Android best practices and generate complete,
working code without placeholders.
```

**User Prompt Pattern:**

```
Generation Type: NewProject
User Request: [User's natural language input]

Existing Project Context:
Name: MyApp
Package: com.example.myapp
Screens: [HomeScreen, SettingsScreen]

Please generate complete code with proper structure.
```

### Response Parsing

Claude responses enthalten Code-Blöcke im Format:

````
```kotlin:path/to/File.kt
package com.example.app

class MainActivity : ComponentActivity() {
    // ...
}
```

```xml:res/layout/activity_main.xml
<?xml version="1.0"?>
<LinearLayout>
    <!-- ... -->
</LinearLayout>
```
````

Parser extrahiert:
1. Sprache/Typ (kotlin, xml, gradle)
2. Dateipfad
3. Code-Inhalt

## Build-System

### MSBuild & .NET SDK

**Build Command:**

```powershell
dotnet build DevBoxAI.sln --configuration Release
```

**Publish Command:**

```powershell
dotnet publish src/DevBoxAI/DevBoxAI.csproj `
  --configuration Release `
  --output build/publish `
  --self-contained true `
  --runtime win-x64 `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true
```

**Resultat:** Single-File Executable `DevBoxAI.exe` (~100-150 MB)

### Android Build (Gradle)

**Debug Build:**

```bash
./gradlew assembleDebug
```

**Release Build:**

```bash
./gradlew assembleRelease
```

**Output:** `app/build/outputs/apk/{debug|release}/app-{debug|release}.apk`

## Testing-Strategie

### Unit Tests (TODO)

```csharp
// DevBoxAI.Core.Tests
[Test]
public void AndroidProject_Creation_ShouldHaveValidPackageName()
{
    var project = new AndroidProject
    {
        Name = "TestApp",
        PackageName = "com.test.app"
    };

    Assert.IsTrue(project.PackageName.Contains("."));
}
```

### Integration Tests (TODO)

```csharp
// DevBoxAI.AI.Tests
[Test]
public async Task ClaudeService_GenerateProject_ShouldReturnValidCode()
{
    var service = new ClaudeCodeGenerationService(apiKey);
    var result = await service.GenerateFromPromptAsync(new GenerationRequest
    {
        Type = GenerationType.NewProject,
        Prompt = "Create a simple counter app"
    });

    Assert.IsTrue(result.Success);
    Assert.IsTrue(result.GeneratedFiles.Count > 0);
}
```

## Erweiterbarkeit

### Neue Integrationen hinzufügen

1. **Enum erweitern** in `DevBoxAI.Core/Models/AndroidProject.cs`:

```csharp
public enum IntegrationType
{
    Firebase,
    Supabase,
    // ... existing
    NewIntegration  // ← Neue Integration
}
```

2. **Prompt erweitern** in `ClaudeCodeGenerationService`:

```csharp
if (project.Integrations.Any(i => i.Type == IntegrationType.NewIntegration))
{
    prompt += "\nInclude NewIntegration SDK setup and configuration.";
}
```

3. **Gradle Dependencies** in `ProjectGenerator`:

```csharp
if (hasNewIntegration)
{
    dependencies += "implementation(\"com.newintegration:sdk:1.0.0\")\n";
}
```

### Neue UI-Komponenten

1. **ComponentType erweitern**:

```csharp
public enum ComponentType
{
    // ... existing
    NewComponent
}
```

2. **Generator-Logic** in `ClaudeCodeGenerationService`:

```csharp
case ComponentType.NewComponent:
    return GenerateNewComponentCode(component);
```

## Performance-Optimierungen

### AI-Request Caching

Implementiere Request-Caching für häufige Anfragen:

```csharp
private readonly Dictionary<string, GenerationResult> _cache = new();

public async Task<GenerationResult> GenerateFromPromptAsync(GenerationRequest request)
{
    var cacheKey = GenerateCacheKey(request);

    if (_cache.TryGetValue(cacheKey, out var cached))
    {
        return cached;
    }

    var result = await CallClaudeAsync(request);
    _cache[cacheKey] = result;

    return result;
}
```

### Async/Await Best Practices

Alle I/O-Operationen verwenden async/await:

```csharp
// ✅ Gut
public async Task<AndroidProject> LoadProjectAsync(string path)
{
    var json = await File.ReadAllTextAsync(path);
    return JsonSerializer.Deserialize<AndroidProject>(json);
}

// ❌ Schlecht
public AndroidProject LoadProject(string path)
{
    var json = File.ReadAllText(path);  // Synchronous I/O blocks UI
    return JsonSerializer.Deserialize<AndroidProject>(json);
}
```

## Debugging

### Logging

Implementiere strukturiertes Logging:

```csharp
private readonly ILogger _logger;

public async Task BuildAsync(AndroidProject project)
{
    _logger.LogInformation("Starting build for project {ProjectName}", project.Name);

    try
    {
        var result = await _buildService.BuildProjectAsync(project);
        _logger.LogInformation("Build completed in {Duration}s", result.BuildDuration.TotalSeconds);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Build failed for project {ProjectName}", project.Name);
        throw;
    }
}
```

### WPF Debugging

XAML Binding Errors debuggen:

```xml
<TextBlock Text="{Binding Path=NonExistentProperty,
                          FallbackValue='DEBUG: Binding Failed'}" />
```

Output Window zeigt:

```
System.Windows.Data Error: 40 : BindingExpression path error:
'NonExistentProperty' property not found on 'object'
```

## Deployment

### Installer erstellen (WiX Toolset)

```xml
<!-- Product.wxs -->
<Product Id="*" Name="DevBoxAI" Version="1.0.0"
         Manufacturer="DevBoxAI Team">
  <Package InstallerVersion="200" Compressed="yes" />

  <Directory Id="TARGETDIR" Name="SourceDir">
    <Directory Id="ProgramFilesFolder">
      <Directory Id="INSTALLFOLDER" Name="DevBoxAI">
        <Component Id="MainExecutable">
          <File Source="DevBoxAI.exe" KeyPath="yes" />
        </Component>
      </Directory>
    </Directory>
  </Directory>
</Product>
```

### Code-Signierung

```powershell
signtool sign /f certificate.pfx /p password /t http://timestamp.digicert.com DevBoxAI.exe
```

## Beitragen

### Code-Style

Folge den .editorconfig-Regeln:

- Indent: 4 Spaces
- Line Endings: CRLF (Windows)
- UTF-8 mit BOM für C#-Dateien

### Pull Request Checklist

- [ ] Code kompiliert ohne Warnings
- [ ] Tests hinzugefügt (wenn relevant)
- [ ] README aktualisiert (wenn API geändert)
- [ ] CHANGELOG.md aktualisiert
- [ ] Keine hardcoded Secrets/API-Keys

---

Für Fragen: support@devboxai.com
