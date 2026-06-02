using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class DeleteModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICatalogService catalogService;

        public DeleteModel(ICatalogService catalogService)
        {
            this.catalogService = catalogService;
        }

        public CatalogItem ProductToDelete { get; private set; }

        public IActionResult OnGet(int id)
        {
            _log.Info($"Now loading... /Catalog/Delete?id={id}");
            ProductToDelete = catalogService.FindCatalogItem(id);

            if (ProductToDelete == null)
            {
                return NotFound();
            }

            return Page();
        }

        public IActionResult OnPost(int id)
        {
            var productToDelete = catalogService.FindCatalogItem(id);

            if (productToDelete != null)
            {
                catalogService.RemoveCatalogItem(productToDelete);
            }

            return RedirectToPage("/Index");
        }
    }
}
