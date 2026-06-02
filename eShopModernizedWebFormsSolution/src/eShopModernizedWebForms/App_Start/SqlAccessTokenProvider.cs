using System.Data.SqlClient;
using Azure.Core;
using Azure.Identity;

namespace eShopModernizedWebForms
{
    public interface ISqlConnectionFactory
    {
        SqlConnection CreateConnection();
    }

    /// <summary>
    /// Obtains an Azure AD access token for SQL using the modern Azure.Identity
    /// SDK (replaces the deprecated Microsoft.Azure.Services.AppAuthentication).
    /// </summary>
    public class ManagedIdentitySqlConnectionFactory : ISqlConnectionFactory
    {
        private static readonly string[] Scopes = { "https://database.windows.net/.default" };
        private readonly DefaultAzureCredential _credential;

        public ManagedIdentitySqlConnectionFactory()
        {
            _credential = new DefaultAzureCredential();
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection
            {
                AccessToken = AccessToken,
                ConnectionString = CatalogConfiguration.CatalogConnectionString
            };
        }

        private string AccessToken
            => _credential.GetToken(new TokenRequestContext(Scopes)).Token;
    }

    public class AppSettingsSqlConnectionFactory : ISqlConnectionFactory
    {
        public SqlConnection CreateConnection()
        {
            return new SqlConnection
            {
                ConnectionString = CatalogConfiguration.CatalogConnectionString
            };
        }
    }
}
