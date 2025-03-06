namespace UI.Configurations;

internal static class Startup
{
    internal static WebApplicationBuilder AddConfigurations(this WebApplicationBuilder builder)
    {
        const string configurationsDirectory = "Configurations";
        var env = builder.Environment;

        builder.Configuration
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
            .AddJsonFile($"{configurationsDirectory}/cache.json", false, true)
            .AddJsonFile($"{configurationsDirectory}/cache.{env.EnvironmentName}.json", true, true)
            .AddJsonFile($"{configurationsDirectory}/middleware.json", false, true)
            .AddJsonFile($"{configurationsDirectory}/middleware.{env.EnvironmentName}.json", true, true)
            .AddJsonFile($"{configurationsDirectory}/security.json", false, true)
            .AddJsonFile($"{configurationsDirectory}/security.{env.EnvironmentName}.json", true, true)
            //.AddJsonFile($"{configurationsDirectory}/securityheaders.json", false, true)
            //.AddJsonFile($"{configurationsDirectory}/securityheaders.{env.EnvironmentName}.json", true, true)
             .AddJsonFile($"{configurationsDirectory}/sendgridmail.json", false, true)
            .AddJsonFile($"{configurationsDirectory}/sendgridmail.{env.EnvironmentName}.json", true, true)
            .AddEnvironmentVariables();

        return builder;
    }
}