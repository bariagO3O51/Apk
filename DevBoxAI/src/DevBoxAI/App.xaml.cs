using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DevBoxAI.Core.Services;
using DevBoxAI.AI.Services;
using DevBoxAI.AndroidGenerator;
using DevBoxAI.ViewModels;
using DevBoxAI.Services;

namespace DevBoxAI;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register Services
        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<ICodeGenerationService>(sp =>
            new ClaudeCodeGenerationService(GetApiKey()));
        services.AddSingleton<IBuildService, BuildService>();
        services.AddSingleton<ProjectGenerator>(sp =>
            new ProjectGenerator(GetWorkspacePath()));

        // Register ViewModels
        services.AddTransient<MainViewModel>();

        // Register Windows
        services.AddTransient<MainWindow>();
    }

    private string GetApiKey()
    {
        // In production, load from secure config
        return Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? "";
    }

    private string GetWorkspacePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var workspace = Path.Combine(appData, "DevBoxAI", "Workspace");
        Directory.CreateDirectory(workspace);
        return workspace;
    }
}
