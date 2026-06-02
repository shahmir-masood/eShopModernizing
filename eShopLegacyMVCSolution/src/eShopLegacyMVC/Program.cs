using System.Data.Entity;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Modules;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace eShopLegacyMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ConfigureLogging();

            var builder = WebApplication.CreateBuilder(args);

            bool useMockData = builder.Configuration.GetValue<bool>("UseMockData");

            builder.Services.AddControllersWithViews();

            // Use Autofac as the DI container (mirrors the old Global.asax setup).
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                containerBuilder.RegisterModule(new ApplicationModule(useMockData));
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Catalog/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();

            // Attribute-routed controllers (e.g. PicController, Web API controllers).
            app.MapControllers();

            // Conventional MVC route.
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Catalog}/{action=Index}/{id?}");

            ConfigureDatabase(app, useMockData);

            app.Run();
        }

        private static void ConfigureLogging()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            var configFile = new FileInfo("log4Net.xml");
            if (configFile.Exists)
            {
                XmlConfigurator.Configure(logRepository, configFile);
            }
        }

        private static void ConfigureDatabase(WebApplication app, bool useMockData)
        {
            if (useMockData)
            {
                return;
            }

            using var scope = app.Services.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<CatalogDBInitializer>();
            Database.SetInitializer<CatalogDBContext>(initializer);
        }
    }
}
