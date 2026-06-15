using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using eShopLegacyMVC.Controllers;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Services;
using eShopLegacyMVC.ViewModel;
using Moq;
using Xunit;

namespace eShopLegacyMVCTests.Controllers
{
    public class CatalogControllerTests
    {
        private Mock<ICatalogService> CreateMockCatalogService()
        {
            var mockService = new Mock<ICatalogService>();
            return mockService;
        }

        private CatalogController CreateControllerWithMockContext(Mock<ICatalogService> mockService)
        {
            var controller = new CatalogController(mockService.Object);
            var context = new Mock<ControllerContext>();
            var request = new Mock<HttpRequestBase>();
            var response = new Mock<HttpResponseBase>();
            var session = new Mock<HttpSessionStateBase>();
            var server = new Mock<HttpServerUtilityBase>();
            var url = new Mock<UrlHelper>();

            request.Setup(r => r.Url).Returns(new System.Uri("http://localhost"));

            context.Setup(c => c.HttpContext).Returns(new HttpContextBaseMock(request.Object, response.Object, session.Object, server.Object));

            controller.ControllerContext = context.Object;
            controller.Url = url.Object;

            return controller;
        }

        [Fact]
        public void Index_ReturnsViewWithPaginatedItems()
        {
            // Arrange
            var mockService = CreateMockCatalogService();
            var paginatedItems = new PaginatedItemsViewModel<CatalogItem>(0, 10, 25, new List<CatalogItem>());
            mockService.Setup(s => s.GetCatalogItemsPaginated(10, 0)).Returns(paginatedItems);

            var controller = CreateControllerWithMockContext(mockService);

            // Act
            var result = controller.Index(10, 0) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(paginatedItems, result.Model);
            mockService.Verify(s => s.GetCatalogItemsPaginated(10, 0), Times.Once);
        }

        [Fact]
        public void Details_ValidId_ReturnsViewWithItem()
        {
            // Arrange
            var mockService = CreateMockCatalogService();
            var item = new CatalogItem { Id = 1, Name = "Test Item" };
            mockService.Setup(s => s.FindCatalogItem(1)).Returns(item);

            var controller = CreateControllerWithMockContext(mockService);

            // Act
            var result = controller.Details(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(item, result.Model);
            mockService.Verify(s => s.FindCatalogItem(1), Times.Once);
        }

        [Fact]
        public void Details_NullId_ReturnsBadRequest()
        {
            // Arrange
            var mockService = CreateMockCatalogService();
            var controller = CreateControllerWithMockContext(mockService);

            // Act
            var result = controller.Details(null) as HttpStatusCodeResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public void Details_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var mockService = CreateMockCatalogService();
            mockService.Setup(s => s.FindCatalogItem(999)).Returns((CatalogItem)null);

            var controller = CreateControllerWithMockContext(mockService);

            // Act
            var result = controller.Details(999) as HttpNotFoundResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Create_Get_ReturnsViewWithSelectLists()
        {
            // Arrange
            var mockService = CreateMockCatalogService();
            var brands = new List<CatalogBrand> { new CatalogBrand { Id = 1, Brand = "Brand 1" } };
            var types = new List<CatalogType> { new CatalogType { Id = 1, Type = "Type 1" } };
            mockService.Setup(s => s.GetCatalogBrands()).Returns(brands);
            mockService.Setup(s => s.GetCatalogTypes()).Returns(types);

            var controller = CreateControllerWithMockContext(mockService);

            // Act
            var result = controller.Create() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ViewBag.CatalogBrandId);
            Assert.NotNull(result.ViewBag.CatalogTypeId);
            mockService.Verify(s => s.GetCatalogBrands(), Times.Once);
            mockService.Verify(s => s.GetCatalogTypes(), Times.Once);
        }

        [Fact]
        public void Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var mockService = CreateMockCatalogService();
            var newItem = new CatalogItem { Name = "New Item", Price = 10.99m };

            var controller = CreateControllerWithMockContext(mockService);

            // Act
            var result = controller.Create(newItem) as RedirectToRouteResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.RouteValues["action"]);
            mockService.Verify(s => s.CreateCatalogItem(newItem), Times.Once);
        }

        [Fact]
        public void Edit_Get_ValidId_ReturnsViewWithItem()
        {
            // Arrange
            var mockService = CreateMockCatalogService();
            var item = new CatalogItem { Id = 1, Name = "Test Item" };
            var brands = new List<CatalogBrand> { new CatalogBrand { Id = 1, Brand = "Brand 1" } };
            var types = new List<CatalogType> { new CatalogType { Id = 1, Type = "Type 1" } };
            mockService.Setup(s => s.FindCatalogItem(1)).Returns(item);
            mockService.Setup(s => s.GetCatalogBrands()).Returns(brands);
            mockService.Setup(s => s.GetCatalogTypes()).Returns(types);

            var controller = CreateControllerWithMockContext(mockService);

            // Act
            var result = controller.Edit(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(item, result.Model);
            mockService.Verify(s => s.FindCatalogItem(1), Times.Once);
        }

        [Fact]
        public void Edit_Get_NullId_ReturnsBadRequest()
        {
            // Arrange
            var mockService = CreateMockCatalogService();
            var controller = CreateControllerWithMockContext(mockService);

            // Act
            var result = controller.Edit((int?)null) as HttpStatusCodeResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public void Edit_Get_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var mockService = CreateMockCatalogService();
            mockService.Setup(s => s.FindCatalogItem(999)).Returns((CatalogItem)null);

            var controller = CreateControllerWithMockContext(mockService);

            // Act
            var result = controller.Edit(999) as HttpNotFoundResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Edit_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var mockService = CreateMockCatalogService();
            var updatedItem = new CatalogItem { Id = 1, Name = "Updated Item", Price = 19.99m };

            var controller = CreateControllerWithMockContext(mockService);

            // Act
            var result = controller.Edit(updatedItem) as RedirectToRouteResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.RouteValues["action"]);
            mockService.Verify(s => s.UpdateCatalogItem(updatedItem), Times.Once);
        }

        [Fact]
        public void Delete_Get_ValidId_ReturnsViewWithItem()
        {
            // Arrange
            var mockService = CreateMockCatalogService();
            var item = new CatalogItem { Id = 1, Name = "Test Item" };
            mockService.Setup(s => s.FindCatalogItem(1)).Returns(item);

            var controller = CreateControllerWithMockContext(mockService);

            // Act
            var result = controller.Delete(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(item, result.Model);
            mockService.Verify(s => s.FindCatalogItem(1), Times.Once);
        }

        [Fact]
        public void Delete_Get_NullId_ReturnsBadRequest()
        {
            // Arrange
            var mockService = CreateMockCatalogService();
            var controller = CreateControllerWithMockContext(mockService);

            // Act
            var result = controller.Delete(null) as HttpStatusCodeResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public void Delete_Get_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var mockService = CreateMockCatalogService();
            mockService.Setup(s => s.FindCatalogItem(999)).Returns((CatalogItem)null);

            var controller = CreateControllerWithMockContext(mockService);

            // Act
            var result = controller.Delete(999) as HttpNotFoundResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void DeleteConfirmed_ValidId_RemovesItemAndRedirects()
        {
            // Arrange
            var mockService = CreateMockCatalogService();
            var item = new CatalogItem { Id = 1, Name = "Test Item" };
            mockService.Setup(s => s.FindCatalogItem(1)).Returns(item);

            var controller = CreateControllerWithMockContext(mockService);

            // Act
            var result = controller.DeleteConfirmed(1) as RedirectToRouteResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.RouteValues["action"]);
            mockService.Verify(s => s.FindCatalogItem(1), Times.Once);
            mockService.Verify(s => s.RemoveCatalogItem(item), Times.Once);
        }

        [Fact]
        public void Dispose_DisposesService()
        {
            // Arrange
            var mockService = CreateMockCatalogService();
            var controller = CreateControllerWithMockContext(mockService);

            // Act
            controller.Dispose();

            // Assert
            mockService.Verify(s => s.Dispose(), Times.Once);
        }

        // Helper class for mocking HttpContext
        private class HttpContextBaseMock : HttpContextBase
        {
            private readonly HttpRequestBase _request;
            private readonly HttpResponseBase _response;
            private readonly HttpSessionStateBase _session;
            private readonly HttpServerUtilityBase _server;

            public HttpContextBaseMock(HttpRequestBase request, HttpResponseBase response, HttpSessionStateBase session, HttpServerUtilityBase server)
            {
                _request = request;
                _response = response;
                _session = session;
                _server = server;
            }

            public override HttpRequestBase Request => _request;
            public override HttpResponseBase Response => _response;
            public override HttpSessionStateBase Session => _session;
            public override HttpServerUtilityBase Server => _server;
        }
    }
}