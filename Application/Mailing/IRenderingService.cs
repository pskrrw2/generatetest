namespace Application.Mailing;

public interface IRenderingService
{
    Task<string> RenderTemplateAsync<T>(string templateName, T templateModel);
}