using Microsoft.Extensions.Configuration;

namespace eShopModernizedMVC
{
    public static class CatalogConfiguration
    {
        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static bool UseMockData => IsEnabled("UseMockData");

        public static bool UseAzureStorage => IsEnabled("UseAzureStorage");

        public static bool UseManagedIdentity => IsEnabled("UseAzureManagedIdentity");

        public static bool UseCustomizationData => IsEnabled("UseCustomizationData");

        public static string StorageConnectionString => GetValue("StorageConnectionString");

        public static string AppInsightsInstrumentationKey => GetValue("AppInsightsInstrumentationKey");

        public static bool UseAzureActiveDirectory => IsEnabled("UseAzureActiveDirectory");

        public static string AzureActiveDirectoryClientId => GetValue("AzureActiveDirectoryClientId");

        public static string AzureActiveDirectoryTenant => GetValue("AzureActiveDirectoryTenant");

        public static string AzureActiveDirectoryInstance => GetValue("AzureActiveDirectoryInstance");

        public static string PostLogoutRedirectUri => GetValue("PostLogoutRedirectUri");

        public static string CatalogConnectionString => _configuration?.GetConnectionString("CatalogDBContext");

        private static string GetValue(string key) => _configuration?[key];

        private static bool IsEnabled(string configurationKey)
        {
            return bool.TryParse(_configuration?[configurationKey], out var enabled) && enabled;
        }
    }
}
