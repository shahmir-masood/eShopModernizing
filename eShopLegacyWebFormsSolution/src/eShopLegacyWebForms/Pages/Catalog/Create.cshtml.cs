using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class CreateModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICatalogService catalogService;

        public CreateModel(ICatalogService catalogService)
        {
            this.catalogService = catalogService;
        }

        [BindProperty]
        public CatalogItem Product { get; set; } = new CatalogItem();

        public SelectList Brands { get; private set; }

        public SelectList Types { get; private set; }

        public void OnGet()
        {
            _log.Info($"Now loading... /Catalog/Create");
            LoadLookups();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                LoadLookups();
                return Page();
            }

            catalogService.CreateCatalogItem(Product);

            return RedirectToPage("/Index");
        }

        private void LoadLookups()
        {
            Brands = new SelectList(catalogService.GetCatalogBrands(), nameof(CatalogBrand.Id), nameof(CatalogBrand.Brand));
            Types = new SelectList(catalogService.GetCatalogTypes(), nameof(CatalogType.Id), nameof(CatalogType.Type));
        }
    }
}
