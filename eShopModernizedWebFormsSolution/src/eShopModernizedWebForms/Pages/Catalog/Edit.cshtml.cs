using System.IO;
using eShopModernizedWebForms.Models;
using eShopModernizedWebForms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace eShopModernizedWebForms.Pages.Catalog
{
    public class EditModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly IImageService _imageService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(ICatalogService catalogService, IImageService imageService, ILogger<EditModel> logger)
        {
            _catalogService = catalogService;
            _imageService = imageService;
            _logger = logger;
        }

        [BindProperty]
        public CatalogItemInputModel Input { get; set; } = new CatalogItemInputModel();

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public SelectList Brands { get; private set; }
        public SelectList Types { get; private set; }
        public string PictureUri { get; private set; }
        public bool ShowUpload => CatalogConfiguration.UseAzureStorage;

        public IActionResult OnGet()
        {
            _logger.LogInformation("Now loading... /Catalog/Edit/{Id}", Id);

            var product = _catalogService.FindCatalogItem(Id);
            if (product == null)
            {
                return NotFound();
            }

            Input = new CatalogItemInputModel
            {
                Name = product.Name,
                Description = product.Description,
                CatalogBrandId = product.CatalogBrandId,
                CatalogTypeId = product.CatalogTypeId,
                Price = product.Price,
                PictureFileName = product.PictureFileName,
                AvailableStock = product.AvailableStock,
                RestockThreshold = product.RestockThreshold,
                MaxStockThreshold = product.MaxStockThreshold
            };

            PictureUri = _imageService.BuildUrlImage(product);
            LoadLookups();
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                LoadLookups();
                return Page();
            }

            var catalogItem = new CatalogItem
            {
                Id = Id,
                Name = Input.Name,
                Description = Input.Description,
                CatalogBrandId = Input.CatalogBrandId,
                CatalogTypeId = Input.CatalogTypeId,
                Price = Input.Price,
                PictureFileName = Input.PictureFileName,
                AvailableStock = Input.AvailableStock,
                RestockThreshold = Input.RestockThreshold,
                MaxStockThreshold = Input.MaxStockThreshold,
                TempImageName = Input.TempImageName
            };

            if (!string.IsNullOrEmpty(catalogItem.TempImageName))
            {
                _imageService.UpdateImage(catalogItem);
                catalogItem.PictureFileName = Path.GetFileName(catalogItem.TempImageName);
            }

            _catalogService.UpdateCatalogItem(catalogItem);

            return RedirectToPage("/Index");
        }

        private void LoadLookups()
        {
            Brands = new SelectList(_catalogService.GetCatalogBrands(), "Id", "Brand", Input.CatalogBrandId);
            Types = new SelectList(_catalogService.GetCatalogTypes(), "Id", "Type", Input.CatalogTypeId);
        }
    }
}
