using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Models.Infrastructure;
using eShopLegacyWebForms.Services;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data.Entity;

var builder = WebApplication.CreateBuilder(args);

bool useMockData = builder.Configuration.GetValue<bool>("UseMockData");

builder.Services.AddRazorPages();

// ICatalogService: mock (in-memory) or EF6-backed implementation
if (useMockData)
{
    builder.Services.AddSingleton<ICatalogService, CatalogServiceMock>();
}
else
{
    builder.Services.AddScoped<ICatalogService, CatalogService>();
}

// EF6 DbContext + supporting services (formerly registered via Autofac ApplicationModule)
var connectionString = builder.Configuration.GetConnectionString("CatalogDBContext");
builder.Services.AddScoped(_ => new CatalogDBContext(connectionString));
builder.Services.AddScoped<CatalogDBInitializer>();
builder.Services.AddSingleton<CatalogItemHiLoGenerator>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

if (!useMockData)
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<CatalogDBInitializer>();
    Database.SetInitializer(initializer);
}

app.Run();
