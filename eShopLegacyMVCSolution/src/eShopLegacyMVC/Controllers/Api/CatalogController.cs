using Microsoft.AspNetCore.Mvc;

namespace eShopLegacyMVC.Controllers.Api
{
    [ApiController]
    [Route("api")]
    public class CatalogController2 : ControllerBase
    {
        [HttpGet]
        public ActionResult Index()
        {
            return new JsonResult(new { Message = "Hello World!" });
        }
    }
}
