using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;

namespace eShopModernizedMVC.Models
{
    /// <summary>
    /// Registers the EF6 SQL Server provider explicitly. This is required when
    /// running Entity Framework 6 on .NET (Core) where providers are not
    /// auto-discovered through machine.config like on .NET Framework.
    /// </summary>
    public class CatalogDBConfiguration : DbConfiguration
    {
        public CatalogDBConfiguration()
        {
            SetProviderServices(SqlProviderServices.ProviderInvariantName, SqlProviderServices.Instance);
            SetProviderFactory(SqlProviderServices.ProviderInvariantName, SqlClientFactory.Instance);
        }
    }
}
