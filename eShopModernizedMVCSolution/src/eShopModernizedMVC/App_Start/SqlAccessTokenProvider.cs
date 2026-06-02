using System;
using System.Data.SqlClient;
using Azure.Core;
using Azure.Identity;

namespace eShopModernizedMVC
{
    public interface ISqlConnectionFactory
    {
        SqlConnection CreateConnection();
    }

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
        {
            get
            {
                var token = _credential.GetToken(new TokenRequestContext(Scopes));
                return token.Token;
            }
        }
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
