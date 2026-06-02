using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using eShopModernizedWebForms.Models;
using eShopModernizedWebForms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace eShopModernizedWebForms.Pages.Catalog
{
    public class CreateModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly IImageService _imageService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(ICatalogService catalogService, IImageService imageService, ILogger<CreateModel> logger)
        {
            _catalogService = catalogService;
            _imageService = imageService;
            _logger = logger;
        }

        [BindProperty]
        public CatalogItemInputModel Input { get; set; } = new CatalogItemInputModel();

        public SelectList Brands { get; private set; }
        public SelectList Types { get; private set; }
        public string PictureUri { get; private set; }
        public bool ShowUpload => CatalogConfiguration.UseAzureStorage;

        public void OnGet()
        {
            _logger.LogInformation("Now loading... /Catalog/Create");
            PictureUri = _imageService.UrlDefaultImage();
            LoadLookups();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                PictureUri = _imageService.UrlDefaultImage();
                LoadLookups();
                return Page();
            }

            var catalogItem = new CatalogItem
            {
                Name = Input.Name,
                Description = Input.Description,
                CatalogBrandId = Input.CatalogBrandId,
                CatalogTypeId = Input.CatalogTypeId,
                Price = Input.Price,
                AvailableStock = Input.AvailableStock,
                RestockThreshold = Input.RestockThreshold,
                MaxStockThreshold = Input.MaxStockThreshold,
                TempImageName = Input.TempImageName
            };

            if (!string.IsNullOrEmpty(catalogItem.TempImageName))
            {
                catalogItem.PictureFileName = Path.GetFileName(catalogItem.TempImageName);
            }

            _catalogService.CreateCatalogItem(catalogItem);

            if (!string.IsNullOrEmpty(catalogItem.TempImageName))
            {
                _imageService.UpdateImage(catalogItem);
            }

            return RedirectToPage("/Index");
        }

        private void LoadLookups()
        {
            Brands = new SelectList(_catalogService.GetCatalogBrands(), "Id", "Brand");
            Types = new SelectList(_catalogService.GetCatalogTypes(), "Id", "Type");
        }
    }

    public class CatalogItemInputModel
    {
        [Required(ErrorMessage = "The Name field is required.")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Display(Name = "Brand")]
        public int CatalogBrandId { get; set; }

        [Display(Name = "Type")]
        public int CatalogTypeId { get; set; }

        [Range(0, 1000000, ErrorMessage = "The Price must be a positive number between 0 and 1 million.")]
        public decimal Price { get; set; }

        public string PictureFileName { get; set; }

        [Range(0, 10000000, ErrorMessage = "The field Stock must be between 0 and 10 million.")]
        [Display(Name = "Stock")]
        public int AvailableStock { get; set; }

        [Range(0, 10000000, ErrorMessage = "The field Restock must be between 0 and 10 million.")]
        [Display(Name = "Restock")]
        public int RestockThreshold { get; set; }

        [Range(0, 10000000, ErrorMessage = "The field Max stock must be between 0 and 10 million.")]
        [Display(Name = "Max stock")]
        public int MaxStockThreshold { get; set; }

        public string TempImageName { get; set; }
    }
}
