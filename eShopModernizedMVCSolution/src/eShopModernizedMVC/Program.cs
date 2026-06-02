using System;
using System.Data.Entity;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Azure.Identity;
using eShopModernizedMVC;
using eShopModernizedMVC.Filters;
using eShopModernizedMVC.Middleware;
using eShopModernizedMVC.Models;
using eShopModernizedMVC.Models.Infrastructure;
using eShopModernizedMVC.Modules;
using eShopModernizedMVC.Services;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Optionally pull secrets from Azure Key Vault when a vault name is configured.
var keyVaultName = builder.Configuration["KeyVaultName"];
if (!string.IsNullOrWhiteSpace(keyVaultName))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{keyVaultName}.vault.azure.net/"),
        new DefaultAzureCredential());
}

// Expose configuration and hosting paths to the static-style helpers used across the app.
CatalogConfiguration.Initialize(builder.Configuration);
HostingConfiguration.Initialize(builder.Environment.ContentRootPath, builder.Environment.WebRootPath);

// log4net configuration.
var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
var log4netConfig = Path.Combine(builder.Environment.ContentRootPath, "log4Net.xml");
if (File.Exists(log4netConfig))
{
    XmlConfigurator.Configure(logRepository, new FileInfo(log4netConfig));
}

// Use Autofac as the DI container.
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new ApplicationModule(
        CatalogConfiguration.UseMockData,
        CatalogConfiguration.UseAzureStorage,
        CatalogConfiguration.UseManagedIdentity));
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ActionTracerFilter>();
});

builder.Services.AddHttpContextAccessor();

// Distributed cache + session (uses Redis when configured, otherwise in-memory).
var redisConnectionString = builder.Configuration["RedisConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}
builder.Services.AddSession();

// Authentication: OpenID Connect + Cookies when Azure AD is enabled.
if (CatalogConfiguration.UseAzureActiveDirectory)
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        var instance = string.IsNullOrEmpty(CatalogConfiguration.AzureActiveDirectoryInstance)
            ? "https://login.microsoftonline.com/{0}"
            : CatalogConfiguration.AzureActiveDirectoryInstance;

        options.ClientId = CatalogConfiguration.AzureActiveDirectoryClientId;
        options.Authority = string.Format(instance, CatalogConfiguration.AzureActiveDirectoryTenant);
        options.SignedOutRedirectUri = CatalogConfiguration.PostLogoutRedirectUri;
        options.ResponseType = "id_token";
        options.Scope.Add("openid");
        options.Scope.Add("profile");
    });
}
else
{
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie();
}

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Shared/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

// Populate per-session diagnostic info shown in the footer.
app.Use(async (context, next) =>
{
    if (string.IsNullOrEmpty(context.Session.GetString("MachineName")))
    {
        context.Session.SetString("MachineName", Environment.MachineName);
        context.Session.SetString("SessionStartTime", DateTime.Now.ToString());
    }
    await next();
});

app.UseAuthentication();

// When Azure AD is disabled, inject a default identity so [Authorize] keeps working.
if (!CatalogConfiguration.UseAzureActiveDirectory)
{
    app.UseMiddleware<AutoAuthenticationMiddleware>();
}

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

InitializeDatabaseAndImages(app);

app.Run();

static void InitializeDatabaseAndImages(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    if (!CatalogConfiguration.UseMockData)
    {
        var hiLoGenerator = services.GetRequiredService<CatalogItemHiLoGenerator>();
        Database.SetInitializer(new CatalogDBInitializer(hiLoGenerator));

        var dbContext = services.GetRequiredService<CatalogDBContext>();
        dbContext.Database.Initialize(force: false);
    }

    if (CatalogConfiguration.UseAzureStorage)
    {
        var imageService = services.GetRequiredService<IImageService>();
        imageService.InitializeCatalogImages();
    }
}
