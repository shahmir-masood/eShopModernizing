using Microsoft.Extensions.Configuration;

namespace eShopModernizedWebForms
{
    /// <summary>
    /// Static accessor over the application configuration. Replaces the old
    /// System.Configuration.ConfigurationManager based implementation so the
    /// existing model/service/initializer code can keep reading settings the
    /// same way after the move to ASP.NET Core on .NET 8.
    /// </summary>
    public static class CatalogConfiguration
    {
        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration, string contentRootPath)
        {
            _configuration = configuration;
            ContentRootPath = contentRootPath;
        }

        public static string ContentRootPath { get; private set; }

        public static bool UseMockData => IsEnabled("UseMockData");

        public static bool UseManagedIdentity => IsEnabled("UseAzureManagedIdentity");

        public static bool UseAzureStorage => IsEnabled("UseAzureStorage");

        public static bool UseCustomizationData => IsEnabled("UseCustomizationData");

        public static string StorageConnectionString => GetValue("StorageConnectionString");

        public static string AppInsightsInstrumentationKey => GetValue("AppInsightsInstrumentationKey");

        public static bool UseAzureActiveDirectory => IsEnabled("UseAzureActiveDirectory");

        public static string AzureActiveDirectoryClientId => GetValue("AzureActiveDirectoryClientId");

        public static string AzureActiveDirectoryTenant => GetValue("AzureActiveDirectoryTenant");

        public static string AzureActiveDirectoryInstance => GetValue("AzureActiveDirectoryInstance");

        public static string PostLogoutRedirectUri => GetValue("PostLogoutRedirectUri");

        public static string KeyVaultName => GetValue("KeyVaultName");

        public static string RedisConnectionString => GetValue("RedisConnectionString");

        public static string CatalogConnectionString
            => _configuration?.GetConnectionString("CatalogDBContext")
               ?? GetValue("ConnectionString");

        private static string GetValue(string key) => _configuration?[key];

        private static bool IsEnabled(string key)
        {
            return bool.TryParse(GetValue(key), out var enabled) && enabled;
        }
    }
}
