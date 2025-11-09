using DevBoxAI.Core.Models;

namespace DevBoxAI.Core.Services;

public interface ICodeGenerationService
{
    Task<GenerationResult> GenerateFromPromptAsync(GenerationRequest request);
    Task<string> GenerateScreenCodeAsync(Screen screen, AndroidProject project);
    Task<string> GenerateViewModelCodeAsync(Screen screen, AndroidProject project);
    Task<string> GenerateLayoutXmlAsync(Screen screen);
    Task<string> GenerateGradleFileAsync(AndroidProject project);
    Task<string> GenerateManifestAsync(AndroidProject project);
}
