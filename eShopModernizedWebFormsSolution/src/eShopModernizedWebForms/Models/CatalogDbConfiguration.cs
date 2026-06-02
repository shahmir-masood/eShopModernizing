using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace eShopModernizedWebForms.Models
{
    /// <summary>
    /// Registers the SQL Server provider for EF6 in code. On .NET (Core) there is
    /// no app.config based provider registration, so this replaces it.
    /// </summary>
    public class CatalogDbConfiguration : DbConfiguration
    {
        public CatalogDbConfiguration()
        {
            SetProviderServices("System.Data.SqlClient", SqlProviderServices.Instance);
            SetProviderFactory("System.Data.SqlClient", System.Data.SqlClient.SqlClientFactory.Instance);
            SetDefaultConnectionFactory(new System.Data.Entity.Infrastructure.SqlConnectionFactory());
        }
    }
}
