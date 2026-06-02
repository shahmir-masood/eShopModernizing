using System.Collections.Generic;
using eShopModernizedWebForms.Models;
using eShopModernizedWebForms.Services;
using eShopModernizedWebForms.ViewModel;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace eShopModernizedWebForms.Pages
{
    public class IndexModel : PageModel
    {
        public const int DefaultPageIndex = 0;
        public const int DefaultPageSize = 10;

        private readonly ICatalogService _catalogService;
        private readonly IImageService _imageService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ICatalogService catalogService, IImageService imageService, ILogger<IndexModel> logger)
        {
            _catalogService = catalogService;
            _imageService = imageService;
            _logger = logger;
        }

        public PaginatedItemsViewModel<CatalogItem> CatalogModel { get; private set; }

        public void OnGet(int? index, int? size)
        {
            _logger.LogInformation("Now loading... /Index");

            var pageIndex = index ?? DefaultPageIndex;
            var pageSize = size ?? DefaultPageSize;

            var paginatedItems = _catalogService.GetCatalogItemsPaginated(pageSize, pageIndex);
            ChangeUriPlaceholder(paginatedItems.Data);
            CatalogModel = paginatedItems;
        }

        private void ChangeUriPlaceholder(IEnumerable<CatalogItem> items)
        {
            foreach (var catalogItem in items)
            {
                catalogItem.PictureUri = _imageService.BuildUrlImage(catalogItem);
            }
        }
    }
}
