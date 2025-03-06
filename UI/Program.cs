using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using UI.Configurations;
using static UI.Areas.Executive.Pages.CreateRequestModel;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfigurations();
builder.AddInfrastructure();
builder.Services.AddAuthentication();

builder.Services.AddRazorPages(options => options.RootDirectory = "/areas")
    .AddNToastNotifyToastr();

builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation()
    .AddMvcOptions(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

builder.Services.AddMemoryCache();

builder.Services.AddValidatorsFromAssemblyContaining<RequestModelValidator>();

builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 5;
    config.IsDismissable = true;
    config.Position = NotyfPosition.TopRight;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/access-denied";
});

builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 443;
});

builder.Services.Configure<MvcOptions>(options =>
{
    options.Filters.Add(new RequireHttpsAttribute());
});

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseNToastNotify();
app.UseNotyf();

app.UseEndpoints(endpoints =>
{
    endpoints?.MapRazorPages();
});

app.MapHealthChecks("/api/health");

await app.Services.InitializeDatabasesAsync();
app.Run();
