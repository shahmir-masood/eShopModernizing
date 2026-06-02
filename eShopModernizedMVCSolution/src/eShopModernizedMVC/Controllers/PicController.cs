using eShopModernizedMVC.Services;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace eShopModernizedMVC.Controllers
{
    public class PicController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly ImageFormat[] ValidFormats = { ImageFormat.Jpeg, ImageFormat.Png, ImageFormat.Gif };
        private readonly IImageService _imageService;

        public PicController(ICatalogService service, IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost]
        [Route("uploadimage")]
        public IActionResult UploadImage()
        {
            _log.Info($"Now processing... /Pic/UploadImage");
            IFormFile image = Request.Form.Files["HelpSectionImages"];
            var itemId = Request.Form["itemId"].ToString();

            if (!IsValidImage(image))
            {
                return BadRequest("image is not valid");
            }

            int.TryParse(itemId, out var catalogItemId);
            var urlImageTemp = _imageService.UploadTempImage(image, catalogItemId);
            var tempImage = new
            {
                name = new Uri(urlImageTemp).PathAndQuery,
                url = urlImageTemp
            };

            return Json(tempImage);
        }

        private bool IsValidImage(IFormFile file)
        {
            if (file == null)
            {
                return false;
            }

            bool isValidImage = true;
            try
            {
                using (var stream = file.OpenReadStream())
                using (var img = Image.FromStream(stream))
                {
                    isValidImage = ValidFormats.Contains(img.RawFormat);
                }
            }
            catch (Exception)
            {
                isValidImage = false;
            }

            return isValidImage;
        }
    }
}
