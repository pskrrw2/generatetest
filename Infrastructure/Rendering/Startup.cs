using Domain.Common.Const;
using Microsoft.Extensions.DependencyInjection;
using RazorLight;
using RazorLight.Extensions;

namespace Infrastructure.Rendering;

internal static class Startup
{
    internal static IServiceCollection AddRendering(this IServiceCollection services)
    {
        services.AddRazorLight(() => new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(
                typeof(Startup).Assembly,
                Constants.DefaultEmailRenderingNamespace)
            .UseMemoryCachingProvider()
            .Build());

        return services;
    }
}