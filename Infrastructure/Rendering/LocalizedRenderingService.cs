using System.Globalization;
using Application.Mailing;
using RazorLight;

namespace Infrastructure.Rendering;

public class LocalizedRenderingService : IRenderingService
{
    private readonly IRazorLightEngine _renderingService;

    public LocalizedRenderingService(IRazorLightEngine renderingService)
    {
        _renderingService = renderingService;
    }

    public Task<string> RenderTemplateAsync<T>(string templateName, T templateModel)
    {
        var lang = "en";
        var templateKey = $"{templateName}_{lang}";

        return _renderingService.CompileRenderAsync(templateKey, templateModel);
    }
}
