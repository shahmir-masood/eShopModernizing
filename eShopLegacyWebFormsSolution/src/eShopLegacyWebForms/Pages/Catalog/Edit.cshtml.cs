using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class EditModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICatalogService catalogService;

        public EditModel(ICatalogService catalogService)
        {
            this.catalogService = catalogService;
        }

        [BindProperty]
        public CatalogItem Product { get; set; }

        public SelectList Brands { get; private set; }

        public SelectList Types { get; private set; }

        public IActionResult OnGet(int id)
        {
            _log.Info($"Now loading... /Catalog/Edit?id={id}");
            Product = catalogService.FindCatalogItem(id);

            if (Product == null)
            {
                return NotFound();
            }

            LoadLookups();
            return Page();
        }

        public IActionResult OnPost(int id)
        {
            if (!ModelState.IsValid)
            {
                LoadLookups();
                return Page();
            }

            Product.Id = id;
            catalogService.UpdateCatalogItem(Product);

            return RedirectToPage("/Index");
        }

        private void LoadLookups()
        {
            Brands = new SelectList(catalogService.GetCatalogBrands(), nameof(CatalogBrand.Id), nameof(CatalogBrand.Brand), Product?.CatalogBrandId);
            Types = new SelectList(catalogService.GetCatalogTypes(), nameof(CatalogType.Id), nameof(CatalogType.Type), Product?.CatalogTypeId);
        }
    }
}
