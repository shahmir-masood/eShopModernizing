using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

// EF6 DbContext configured via DI.
builder.Services.AddScoped<EntityModel>(_ =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("EntityModel")
        ?? CatalogConfiguration.ConnectionString;
    return new EntityModel(connectionString);
});

// WCF service implementation resolved from DI.
builder.Services.AddScoped<CatalogService>();

var app = builder.Build();

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>();
    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        new BasicHttpBinding(), "/CatalogService.svc");

    var metadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    metadataBehavior.HttpGetEnabled = true;
});

app.Run();
