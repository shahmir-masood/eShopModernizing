using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class DetailsModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICatalogService catalogService;

        public DetailsModel(ICatalogService catalogService)
        {
            this.catalogService = catalogService;
        }

        public CatalogItem Product { get; private set; }

        public IActionResult OnGet(int id)
        {
            _log.Info($"Now loading... /Catalog/Details?id={id}");
            Product = catalogService.FindCatalogItem(id);

            if (Product == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
