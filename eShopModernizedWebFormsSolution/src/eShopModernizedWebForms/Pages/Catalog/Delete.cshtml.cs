using eShopModernizedWebForms.Models;
using eShopModernizedWebForms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace eShopModernizedWebForms.Pages.Catalog
{
    public class DeleteModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly IImageService _imageService;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(ICatalogService catalogService, IImageService imageService, ILogger<DeleteModel> logger)
        {
            _catalogService = catalogService;
            _imageService = imageService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public CatalogItem ProductToDelete { get; private set; }
        public string PictureUri { get; private set; }

        public IActionResult OnGet()
        {
            _logger.LogInformation("Now loading... /Catalog/Delete/{Id}", Id);

            ProductToDelete = _catalogService.FindCatalogItem(Id);
            if (ProductToDelete == null)
            {
                return NotFound();
            }

            PictureUri = _imageService.BuildUrlImage(ProductToDelete);
            return Page();
        }

        public IActionResult OnPost()
        {
            var product = _catalogService.FindCatalogItem(Id);
            if (product != null)
            {
                _catalogService.RemoveCatalogItem(product);
            }

            return RedirectToPage("/Index");
        }
    }
}
