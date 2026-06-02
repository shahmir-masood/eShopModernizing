using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using eShopModernizedWebForms;
using eShopModernizedWebForms.Models;
using eShopModernizedWebForms.Models.Infrastructure;
using eShopModernizedWebForms.Services;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Optional Azure Key Vault configuration source (replaces the old
// OptionalKeyVaultConfigurationBuilder / config builders feature).
var keyVaultName = builder.Configuration["KeyVaultName"];
if (!string.IsNullOrWhiteSpace(keyVaultName))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{keyVaultName}.vault.azure.net/"),
        new DefaultAzureCredential());
}

CatalogConfiguration.Initialize(builder.Configuration, builder.Environment.ContentRootPath);

// Dependency injection (replaces the old Autofac ApplicationModule).
if (CatalogConfiguration.UseManagedIdentity)
{
    builder.Services.AddSingleton<ISqlConnectionFactory, ManagedIdentitySqlConnectionFactory>();
}
else
{
    builder.Services.AddSingleton<ISqlConnectionFactory, AppSettingsSqlConnectionFactory>();
}

builder.Services.AddSingleton<CatalogItemHiLoGenerator>();
builder.Services.AddScoped<CatalogDBContext>();
builder.Services.AddScoped<CatalogDBInitializer>();

if (CatalogConfiguration.UseMockData)
{
    builder.Services.AddSingleton<ICatalogService, CatalogServiceMock>();
}
else
{
    builder.Services.AddScoped<ICatalogService, CatalogService>();
}

if (CatalogConfiguration.UseAzureStorage)
{
    builder.Services.AddScoped<IImageService, ImageAzureStorage>();
}
else
{
    builder.Services.AddScoped<IImageService, ImageMockStorage>();
}

// Authentication: ASP.NET Core cookies + OpenID Connect (replaces OWIN).
var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CatalogConfiguration.UseAzureActiveDirectory
        ? OpenIdConnectDefaults.AuthenticationScheme
        : CookieAuthenticationDefaults.AuthenticationScheme;
});
authBuilder.AddCookie();
if (CatalogConfiguration.UseAzureActiveDirectory)
{
    var instance = CatalogConfiguration.AzureActiveDirectoryInstance ?? "https://login.microsoftonline.com/{0}";
    authBuilder.AddOpenIdConnect(options =>
    {
        options.ClientId = CatalogConfiguration.AzureActiveDirectoryClientId;
        options.Authority = string.Format(CultureInfo.InvariantCulture, instance, CatalogConfiguration.AzureActiveDirectoryTenant);
        options.SignedOutRedirectUri = CatalogConfiguration.PostLogoutRedirectUri;
        options.Events = new OpenIdConnectEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.HandleResponse();
                context.Response.Redirect("/Error?message=" + context.Exception.Message);
                return System.Threading.Tasks.Task.CompletedTask;
            }
        };
    });
}
builder.Services.AddAuthorization();

// Distributed cache + session (replaces Microsoft.Web.RedisSessionStateProvider).
var redisConnection = CatalogConfiguration.RedisConnectionString;
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);
}
else
{
    builder.Services.AddDistributedMemoryCache();
}
builder.Services.AddSession();

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure EF6 database initialization for the SQL-backed scenario.
if (!CatalogConfiguration.UseMockData)
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<CatalogDBInitializer>();
    Database.SetInitializer(initializer);
}

// Initialize catalog images (no-op for mock storage).
using (var scope = app.Services.CreateScope())
{
    var imageService = scope.ServiceProvider.GetRequiredService<IImageService>();
    imageService.InitializeCatalogImages();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();

// Image upload endpoint (replaces the old PicUploader.asmx web service).
app.MapPost("/api/picupload", async (HttpRequest request, IImageService imageService) =>
{
    if (!request.HasFormContentType)
        return Results.BadRequest("image is not valid");

    var form = await request.ReadFormAsync();
    var file = form.Files["HelpSectionImages"] ?? form.Files.FirstOrDefault();

    if (file == null || file.Length == 0)
        return Results.BadRequest("image is not valid");

    int.TryParse(form["itemId"], out var catalogItemId);
    var urlImageTemp = imageService.UploadTempImage(file, catalogItemId);

    return Results.Json(new { name = urlImageTemp, url = urlImageTemp });
});

app.Run();
