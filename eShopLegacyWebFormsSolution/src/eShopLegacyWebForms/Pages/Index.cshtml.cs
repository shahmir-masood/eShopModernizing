using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using eShopLegacyWebForms.ViewModel;
using log4net;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;

namespace eShopLegacyWebForms.Pages
{
    public class IndexModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const int DefaultPageIndex = 0;
        public const int DefaultPageSize = 10;

        private readonly ICatalogService catalogService;

        public IndexModel(ICatalogService catalogService)
        {
            this.catalogService = catalogService;
        }

        public PaginatedItemsViewModel<CatalogItem> ProductList { get; private set; }

        public void OnGet(int? index, int? size)
        {
            var pageIndex = index ?? DefaultPageIndex;
            var pageSize = size ?? DefaultPageSize;

            ProductList = catalogService.GetCatalogItemsPaginated(pageSize, pageIndex);
            _log.Info($"Now loading... /Index?size={pageSize}&index={pageIndex}");
        }
    }
}
