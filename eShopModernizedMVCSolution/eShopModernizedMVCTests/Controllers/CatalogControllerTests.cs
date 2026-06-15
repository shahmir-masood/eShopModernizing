using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using eShopModernizedMVC.Controllers;
using eShopModernizedMVC.Models;
using eShopModernizedMVC.Services;
using eShopModernizedMVC.ViewModel;
using Moq;
using Xunit;

namespace eShopModernizedMVCTests.Controllers
{
    public class CatalogControllerTests
    {
        private static Mock<IImageService> CreateImageService()
        {
            var image = new Mock<IImageService>();
            image.Setup(i => i.BuildUrlImage(It.IsAny<CatalogItem>())).Returns("http://image/url.png");
            image.Setup(i => i.UrlDefaultImage()).Returns("http://image/default.png");
            return image;
        }

        [Fact]
        public void Index_ReturnsViewWithPaginatedItems()
        {
            var service = new Mock<ICatalogService>();
            var items = new List<CatalogItem> { new CatalogItem { Id = 1 }, new CatalogItem { Id = 2 } };
            var paginated = new PaginatedItemsViewModel<CatalogItem>(0, 10, 2, items);
            service.Setup(s => s.GetCatalogItemsPaginated(10, 0)).Returns(paginated);
            var image = CreateImageService();
            var controller = new CatalogController(service.Object, image.Object);

            var result = controller.Index(10, 0) as ViewResult;

            Assert.NotNull(result);
            Assert.Equal(paginated, result.Model);
            image.Verify(i => i.BuildUrlImage(It.IsAny<CatalogItem>()), Times.Exactly(2));
        }

        [Fact]
        public void Details_NullId_ReturnsBadRequest()
        {
            var controller = new CatalogController(new Mock<ICatalogService>().Object, CreateImageService().Object);

            var result = controller.Details(null) as HttpStatusCodeResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public void Details_ValidId_ReturnsViewWithItem()
        {
            var service = new Mock<ICatalogService>();
            var item = new CatalogItem { Id = 1, Name = "Test" };
            service.Setup(s => s.FindCatalogItem(1)).Returns(item);
            var controller = new CatalogController(service.Object, CreateImageService().Object);

            var result = controller.Details(1) as ViewResult;

            Assert.NotNull(result);
            Assert.Equal(item, result.Model);
        }

        [Fact]
        public void Details_InvalidId_ReturnsNotFound()
        {
            var service = new Mock<ICatalogService>();
            service.Setup(s => s.FindCatalogItem(999)).Returns((CatalogItem)null);
            var controller = new CatalogController(service.Object, CreateImageService().Object);

            var result = controller.Details(999);

            Assert.IsType<HttpNotFoundResult>(result);
        }

        [Fact]
        public void Create_Get_ReturnsViewWithDefaultImage()
        {
            var service = new Mock<ICatalogService>();
            service.Setup(s => s.GetCatalogBrands()).Returns(new List<CatalogBrand>());
            service.Setup(s => s.GetCatalogTypes()).Returns(new List<CatalogType>());
            var image = CreateImageService();
            var controller = new CatalogController(service.Object, image.Object);

            var result = controller.Create() as ViewResult;

            Assert.NotNull(result);
            var model = Assert.IsType<CatalogItem>(result.Model);
            Assert.Equal("http://image/default.png", model.PictureUri);
            Assert.NotNull(result.ViewBag.CatalogBrandId);
            Assert.NotNull(result.ViewBag.CatalogTypeId);
        }

        [Fact]
        public void Create_Post_ValidModel_RedirectsToIndex()
        {
            var service = new Mock<ICatalogService>();
            var image = CreateImageService();
            var controller = new CatalogController(service.Object, image.Object);
            var newItem = new CatalogItem { Name = "New Item", Price = 5m };

            var result = controller.Create(newItem) as RedirectToRouteResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.RouteValues["action"]);
            service.Verify(s => s.CreateCatalogItem(newItem), Times.Once);
        }

        [Fact]
        public void Create_Post_InvalidModel_ReturnsView()
        {
            var service = new Mock<ICatalogService>();
            service.Setup(s => s.GetCatalogBrands()).Returns(new List<CatalogBrand>());
            service.Setup(s => s.GetCatalogTypes()).Returns(new List<CatalogType>());
            var controller = new CatalogController(service.Object, CreateImageService().Object);
            controller.ModelState.AddModelError("Name", "Required");
            var item = new CatalogItem();

            var result = controller.Create(item) as ViewResult;

            Assert.NotNull(result);
            Assert.Equal(item, result.Model);
            service.Verify(s => s.CreateCatalogItem(It.IsAny<CatalogItem>()), Times.Never);
        }

        [Fact]
        public void Edit_Get_NullId_ReturnsBadRequest()
        {
            var controller = new CatalogController(new Mock<ICatalogService>().Object, CreateImageService().Object);

            var result = controller.Edit((int?)null) as HttpStatusCodeResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public void Edit_Get_ValidId_ReturnsViewWithItem()
        {
            var service = new Mock<ICatalogService>();
            var item = new CatalogItem { Id = 1, Name = "Test" };
            service.Setup(s => s.FindCatalogItem(1)).Returns(item);
            service.Setup(s => s.GetCatalogBrands()).Returns(new List<CatalogBrand>());
            service.Setup(s => s.GetCatalogTypes()).Returns(new List<CatalogType>());
            var controller = new CatalogController(service.Object, CreateImageService().Object);

            var result = controller.Edit(1) as ViewResult;

            Assert.NotNull(result);
            Assert.Equal(item, result.Model);
        }

        [Fact]
        public void Edit_Get_InvalidId_ReturnsNotFound()
        {
            var service = new Mock<ICatalogService>();
            service.Setup(s => s.FindCatalogItem(999)).Returns((CatalogItem)null);
            var controller = new CatalogController(service.Object, CreateImageService().Object);

            var result = controller.Edit(999);

            Assert.IsType<HttpNotFoundResult>(result);
        }

        [Fact]
        public void Edit_Post_ValidModel_RedirectsToIndex()
        {
            var service = new Mock<ICatalogService>();
            var controller = new CatalogController(service.Object, CreateImageService().Object);
            var item = new CatalogItem { Id = 1, Name = "Updated" };

            var result = controller.Edit(item) as RedirectToRouteResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.RouteValues["action"]);
            service.Verify(s => s.UpdateCatalogItem(item), Times.Once);
        }

        [Fact]
        public void Delete_Get_NullId_ReturnsBadRequest()
        {
            var controller = new CatalogController(new Mock<ICatalogService>().Object, CreateImageService().Object);

            var result = controller.Delete(null) as HttpStatusCodeResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public void Delete_Get_ValidId_ReturnsViewWithItem()
        {
            var service = new Mock<ICatalogService>();
            var item = new CatalogItem { Id = 1, Name = "Test" };
            service.Setup(s => s.FindCatalogItem(1)).Returns(item);
            var controller = new CatalogController(service.Object, CreateImageService().Object);

            var result = controller.Delete(1) as ViewResult;

            Assert.NotNull(result);
            Assert.Equal(item, result.Model);
        }

        [Fact]
        public void Delete_Get_InvalidId_ReturnsNotFound()
        {
            var service = new Mock<ICatalogService>();
            service.Setup(s => s.FindCatalogItem(999)).Returns((CatalogItem)null);
            var controller = new CatalogController(service.Object, CreateImageService().Object);

            var result = controller.Delete(999);

            Assert.IsType<HttpNotFoundResult>(result);
        }

        [Fact]
        public void DeleteConfirmed_RemovesItemAndRedirects()
        {
            var service = new Mock<ICatalogService>();
            var item = new CatalogItem { Id = 1 };
            service.Setup(s => s.FindCatalogItem(1)).Returns(item);
            var controller = new CatalogController(service.Object, CreateImageService().Object);

            var result = controller.DeleteConfirmed(1) as RedirectToRouteResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.RouteValues["action"]);
            service.Verify(s => s.RemoveCatalogItem(item), Times.Once);
        }
    }
}
