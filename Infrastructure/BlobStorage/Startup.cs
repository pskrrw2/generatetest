using Application.BlobStorage;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.BlobStorage;

internal static class Startup
{
    internal static IServiceCollection AddBlobStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped(_ => new BlobServiceClient(configuration.GetConnectionString("AzureBlobStorage")));
        services.AddScoped<IBlobStorageService, BlobStorageService>();

        return services;
    }
}
