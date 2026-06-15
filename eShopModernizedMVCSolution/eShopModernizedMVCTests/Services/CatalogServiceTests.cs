using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using eShopModernizedMVC;
using eShopModernizedMVC.Models;
using eShopModernizedMVC.Services;
using Moq;
using Xunit;

namespace eShopModernizedMVCTests.Services
{
    public class CatalogServiceTests
    {
        private static Mock<CatalogDBContext> CreateMockContext()
        {
            var factory = new Mock<ISqlConnectionFactory>();
            factory.Setup(f => f.CreateConnection()).Returns(() => new SqlConnection());
            return new Mock<CatalogDBContext>(factory.Object);
        }

        private static List<CatalogItem> BuildItems(int count)
        {
            var items = new List<CatalogItem>();
            for (int i = 1; i <= count; i++)
            {
                items.Add(new CatalogItem
                {
                    Id = i,
                    Name = "Item " + i,
                    CatalogBrand = new CatalogBrand { Id = 1, Brand = "Brand 1" },
                    CatalogType = new CatalogType { Id = 1, Type = "Type 1" }
                });
            }
            return items;
        }

        private static Mock<CatalogDBContext> CreateContext(
            TestDbSet<CatalogItem> items = null,
            TestDbSet<CatalogType> types = null,
            TestDbSet<CatalogBrand> brands = null)
        {
            var ctx = CreateMockContext();
            if (items != null) ctx.Setup(c => c.CatalogItems).Returns(items);
            if (types != null) ctx.Setup(c => c.CatalogTypes).Returns(types);
            if (brands != null) ctx.Setup(c => c.CatalogBrands).Returns(brands);
            ctx.Setup(c => c.SaveChanges()).Returns(1);
            return ctx;
        }

        [Fact]
        public void GetCatalogItemsPaginated_ReturnsCorrectPage()
        {
            var set = new TestDbSet<CatalogItem>(BuildItems(5));
            var ctx = CreateContext(items: set);
            var service = new CatalogService(ctx.Object, new Mock<CatalogItemHiLoGenerator>().Object);

            var result = service.GetCatalogItemsPaginated(pageSize: 2, pageIndex: 0);

            Assert.NotNull(result);
            Assert.Equal(0, result.ActualPage);
            Assert.Equal(2, result.ItemsPerPage);
            Assert.Equal(5, result.TotalItems);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(1, result.Data.First().Id);
        }

        [Fact]
        public void GetCatalogItemsPaginated_SecondPage_ReturnsCorrectItems()
        {
            var set = new TestDbSet<CatalogItem>(BuildItems(5));
            var ctx = CreateContext(items: set);
            var service = new CatalogService(ctx.Object, new Mock<CatalogItemHiLoGenerator>().Object);

            var result = service.GetCatalogItemsPaginated(pageSize: 2, pageIndex: 1);

            Assert.Equal(1, result.ActualPage);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(3, result.Data.First().Id);
        }

        [Fact]
        public void FindCatalogItem_ExistingId_ReturnsItem()
        {
            var set = new TestDbSet<CatalogItem>(BuildItems(2));
            var ctx = CreateContext(items: set);
            var service = new CatalogService(ctx.Object, new Mock<CatalogItemHiLoGenerator>().Object);

            var result = service.FindCatalogItem(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void FindCatalogItem_NonExistingId_ReturnsNull()
        {
            var set = new TestDbSet<CatalogItem>(BuildItems(1));
            var ctx = CreateContext(items: set);
            var service = new CatalogService(ctx.Object, new Mock<CatalogItemHiLoGenerator>().Object);

            var result = service.FindCatalogItem(999);

            Assert.Null(result);
        }

        [Fact]
        public void GetCatalogTypes_ReturnsAllTypes()
        {
            var types = new TestDbSet<CatalogType>(new List<CatalogType>
            {
                new CatalogType { Id = 1, Type = "Type 1" },
                new CatalogType { Id = 2, Type = "Type 2" }
            });
            var ctx = CreateContext(types: types);
            var service = new CatalogService(ctx.Object, new Mock<CatalogItemHiLoGenerator>().Object);

            var result = service.GetCatalogTypes();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void GetCatalogBrands_ReturnsAllBrands()
        {
            var brands = new TestDbSet<CatalogBrand>(new List<CatalogBrand>
            {
                new CatalogBrand { Id = 1, Brand = "Brand 1" },
                new CatalogBrand { Id = 2, Brand = "Brand 2" }
            });
            var ctx = CreateContext(brands: brands);
            var service = new CatalogService(ctx.Object, new Mock<CatalogItemHiLoGenerator>().Object);

            var result = service.GetCatalogBrands();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void CreateCatalogItem_AssignsIdAndPersists()
        {
            var set = new TestDbSet<CatalogItem>();
            var ctx = CreateContext(items: set);
            var generator = new Mock<CatalogItemHiLoGenerator>();
            generator.Setup(g => g.GetNextSequenceValue(It.IsAny<CatalogDBContext>())).Returns(10);
            var service = new CatalogService(ctx.Object, generator.Object);
            var newItem = new CatalogItem { Name = "New Item", Price = 10.99m };

            service.CreateCatalogItem(newItem);

            Assert.Equal(10, newItem.Id);
            Assert.Contains(newItem, set.Items);
            generator.Verify(g => g.GetNextSequenceValue(ctx.Object), Times.Once);
            ctx.Verify(c => c.SaveChanges(), Times.Once);
        }

        [Fact]
        public void RemoveCatalogItem_RemovesItemAndPersists()
        {
            var item = new CatalogItem { Id = 1, Name = "Item 1" };
            var set = new TestDbSet<CatalogItem>(new[] { item });
            var ctx = CreateContext(items: set);
            var service = new CatalogService(ctx.Object, new Mock<CatalogItemHiLoGenerator>().Object);

            service.RemoveCatalogItem(item);

            Assert.DoesNotContain(item, set.Items);
            ctx.Verify(c => c.SaveChanges(), Times.Once);
        }
    }
}
