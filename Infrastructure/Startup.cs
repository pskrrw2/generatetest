using Application.ICurrentUserService;
using Application.IDataService;
using Application.Mailing;
using Application.Service;
using Domain.Identity;
using Infrastructure.BlobStorage;
using Infrastructure.Context;
using Infrastructure.DataService;
using Infrastructure.Initialization;
using Infrastructure.Mailing;
using Infrastructure.Rendering;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class Startup
{
    public static IServiceCollection AddInfrastructure(this WebApplicationBuilder builder)
    {
        using ConfigurationManager config = builder.Configuration;

        builder.WebHost.UseStaticWebAssets();

        return builder
            .Services
            .ConfigureInfrastructureServices(config)
            .AddPersistence(config)
            .AddRendering()
            .AddBlobStorageServices(config)
            .AddRouting(options => options.LowercaseUrls = true)
            .AddHealthCheck()
            .AddMailing(config);
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        return services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, x =>
                {
                    x.MigrationsAssembly("Infrastructure");
                    x.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    x.EnableRetryOnFailure();
                }))
                .AddTransient<ApplicationDbInitializer>()
                .AddTransient<ApplicationDbSeeder>()
                .AddServices(typeof(ICustomSeeder), ServiceLifetime.Transient)
                .AddTransient<CustomSeederRunner>(); ;
    }

    public static IServiceCollection ConfigureInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

        // Add additional Identity options if needed
        services.Configure<IdentityOptions>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
        });

        services.ConfigureApplicationCookie(options =>
        {
            // Cookie settings
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            options.LoginPath = "/Identity/Account/Login";
            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            options.SlidingExpiration = true;
        });

        services.AddTransient<ApplicationDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IMatchEventService, MatchEventService>();
        services.AddScoped<IRenderingService, LocalizedRenderingService>();
        services.AddScoped<IMailService, SendGridMailService>();
        services.AddScoped<IMailManagerService, MailManagerService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IEventRequestService, EventRequestService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, Infrastructure.CurrentUserService.CurrentUserService>();
        services.AddScoped<IAttendeeService, AttendeeService>();
        services.AddScoped<IConferenceService, ConferenceService>();
        services.AddScoped<IContactUsService, ContactUsService>();
        services.AddScoped<IAddOnMasterService, AddOnMasterService>();

        return services;
    }

    public static async Task InitializeDatabasesAsync(
     this IServiceProvider services,
     CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<ApplicationDbInitializer>().InitializeAsync(cancellationToken);
    }

    private static IServiceCollection AddHealthCheck(this IServiceCollection services)
    {
        return services.AddHealthChecks().Services;
    }
}
