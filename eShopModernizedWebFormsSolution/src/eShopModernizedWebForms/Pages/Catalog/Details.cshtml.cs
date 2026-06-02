using eShopModernizedWebForms.Models;
using eShopModernizedWebForms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace eShopModernizedWebForms.Pages.Catalog
{
    public class DetailsModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly IImageService _imageService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(ICatalogService catalogService, IImageService imageService, ILogger<DetailsModel> logger)
        {
            _catalogService = catalogService;
            _imageService = imageService;
            _logger = logger;
        }

        public CatalogItem Product { get; private set; }
        public string PictureUri { get; private set; }

        public IActionResult OnGet(int id)
        {
            _logger.LogInformation("Now loading... /Catalog/Details/{Id}", id);

            Product = _catalogService.FindCatalogItem(id);
            if (Product == null)
            {
                return NotFound();
            }

            PictureUri = _imageService.BuildUrlImage(Product);
            return Page();
        }
    }
}
