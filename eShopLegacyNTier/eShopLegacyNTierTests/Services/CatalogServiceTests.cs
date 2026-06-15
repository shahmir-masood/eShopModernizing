using System;
using System.Collections.Generic;
using System.Linq;
using eShopWCFService;
using eShopWCFService.Models;
using Moq;
using Xunit;

namespace eShopLegacyNTierTests.Services
{
    public class CatalogServiceTests
    {
        private static Mock<EntityModel> CreateModel(
            TestDbSet<CatalogItem> items = null,
            TestDbSet<CatalogBrand> brands = null,
            TestDbSet<CatalogType> types = null,
            TestDbSet<DiscountItem> discounts = null,
            TestDbSet<CatalogItemsStock> stocks = null)
        {
            var model = new Mock<EntityModel>();
            if (items != null) model.Setup(m => m.CatalogItems).Returns(items);
            if (brands != null) model.Setup(m => m.CatalogBrands).Returns(brands);
            if (types != null) model.Setup(m => m.CatalogTypes).Returns(types);
            if (discounts != null) model.Setup(m => m.DiscountItems).Returns(discounts);
            if (stocks != null) model.Setup(m => m.CatalogItemsStocks).Returns(stocks);
            model.Setup(m => m.SaveChanges()).Returns(1);
            return model;
        }

        [Fact]
        public void GetDiscount_ExistingDiscount_ReturnsDiscount()
        {
            var discounts = new TestDbSet<DiscountItem>(new[]
            {
                new DiscountItem { Id = 1, Start = new DateTime(2024, 1, 1), End = new DateTime(2024, 1, 31), Size = 10 }
            });
            var model = CreateModel(discounts: discounts);
            var service = new CatalogService(model.Object);

            var result = service.GetDiscount(new DateTime(2024, 1, 15));

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void GetDiscount_NoDiscountForDate_ReturnsNull()
        {
            var discounts = new TestDbSet<DiscountItem>(new[]
            {
                new DiscountItem { Id = 1, Start = new DateTime(2024, 1, 1), End = new DateTime(2024, 1, 31), Size = 10 }
            });
            var model = CreateModel(discounts: discounts);
            var service = new CatalogService(model.Object);

            var result = service.GetDiscount(new DateTime(2024, 12, 15));

            Assert.Null(result);
        }

        [Fact]
        public void FindCatalogItem_ExistingId_ReturnsItemWithNavigation()
        {
            var items = new TestDbSet<CatalogItem>(new[]
            {
                new CatalogItem { Id = 1, CatalogBrandId = 1, CatalogTypeId = 1 }
            });
            var brands = new TestDbSet<CatalogBrand>(new[] { new CatalogBrand { Id = 1, Brand = "Brand 1" } });
            var types = new TestDbSet<CatalogType>(new[] { new CatalogType { Id = 1, Type = "Type 1" } });
            var model = CreateModel(items: items, brands: brands, types: types);
            var service = new CatalogService(model.Object);

            var result = service.FindCatalogItem(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.NotNull(result.CatalogBrand);
            Assert.NotNull(result.CatalogType);
        }

        [Fact]
        public void FindCatalogItem_NonExistingId_ReturnsNull()
        {
            var items = new TestDbSet<CatalogItem>(new[] { new CatalogItem { Id = 1 } });
            var model = CreateModel(items: items);
            var service = new CatalogService(model.Object);

            var result = service.FindCatalogItem(999);

            Assert.Null(result);
        }

        [Fact]
        public void GetCatalogTypes_ReturnsAllTypes()
        {
            var types = new TestDbSet<CatalogType>(new[]
            {
                new CatalogType { Id = 1, Type = "Type 1" },
                new CatalogType { Id = 2, Type = "Type 2" }
            });
            var model = CreateModel(types: types);
            var service = new CatalogService(model.Object);

            var result = service.GetCatalogTypes();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetCatalogBrands_ReturnsAllBrands()
        {
            var brands = new TestDbSet<CatalogBrand>(new[]
            {
                new CatalogBrand { Id = 1, Brand = "Brand 1" },
                new CatalogBrand { Id = 2, Brand = "Brand 2" }
            });
            var model = CreateModel(brands: brands);
            var service = new CatalogService(model.Object);

            var result = service.GetCatalogBrands();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetCatalogItems_NoFilter_ReturnsAll()
        {
            var items = new TestDbSet<CatalogItem>(new[]
            {
                new CatalogItem { Id = 1, CatalogBrandId = 1, CatalogTypeId = 1 },
                new CatalogItem { Id = 2, CatalogBrandId = 2, CatalogTypeId = 2 }
            });
            var model = CreateModel(items: items);
            var service = new CatalogService(model.Object);

            var result = service.GetCatalogItems(0, 0);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetCatalogItems_BrandFilter_ReturnsMatching()
        {
            var items = new TestDbSet<CatalogItem>(new[]
            {
                new CatalogItem { Id = 1, CatalogBrandId = 1, CatalogTypeId = 1 },
                new CatalogItem { Id = 2, CatalogBrandId = 2, CatalogTypeId = 2 }
            });
            var model = CreateModel(items: items);
            var service = new CatalogService(model.Object);

            var result = service.GetCatalogItems(1, 0);

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public void GetCatalogItems_TypeFilter_ReturnsMatching()
        {
            var items = new TestDbSet<CatalogItem>(new[]
            {
                new CatalogItem { Id = 1, CatalogBrandId = 1, CatalogTypeId = 1 },
                new CatalogItem { Id = 2, CatalogBrandId = 2, CatalogTypeId = 2 }
            });
            var model = CreateModel(items: items);
            var service = new CatalogService(model.Object);

            var result = service.GetCatalogItems(0, 2);

            Assert.Single(result);
            Assert.Equal(2, result[0].Id);
        }

        [Fact]
        public void CreateCatalogItem_AssignsNextIdAndPersists()
        {
            var set = new TestDbSet<CatalogItem>(new[]
            {
                new CatalogItem { Id = 5 }
            });
            var model = CreateModel(items: set);
            var service = new CatalogService(model.Object);
            var newItem = new CatalogItem { Name = "New" };

            service.CreateCatalogItem(newItem);

            Assert.Equal(6, newItem.Id);
            Assert.Contains(newItem, set.Items);
            model.Verify(m => m.SaveChanges(), Times.Once);
        }

        [Fact]
        public void RemoveCatalogItem_RemovesAndPersists()
        {
            var item = new CatalogItem { Id = 1 };
            var set = new TestDbSet<CatalogItem>(new[] { item });
            var model = CreateModel(items: set);
            var service = new CatalogService(model.Object);

            service.RemoveCatalogItem(item);

            Assert.DoesNotContain(item, set.Items);
            model.Verify(m => m.SaveChanges(), Times.Once);
        }

        [Fact]
        public void GetAvailableStock_ExistingStock_ReturnsAvailable()
        {
            var stocks = new TestDbSet<CatalogItemsStock>(new[]
            {
                new CatalogItemsStock { StockId = 1, CatalogItemId = 1, Date = new DateTime(2024, 1, 15), AvailableStock = 42 }
            });
            var model = CreateModel(stocks: stocks);
            var service = new CatalogService(model.Object);

            var result = service.GetAvailableStock(new DateTime(2024, 1, 15), 1);

            Assert.Equal(42, result);
        }

        [Fact]
        public void GetAvailableStock_NoStock_ReturnsZero()
        {
            var stocks = new TestDbSet<CatalogItemsStock>();
            var model = CreateModel(stocks: stocks);
            var service = new CatalogService(model.Object);

            var result = service.GetAvailableStock(new DateTime(2024, 1, 15), 1);

            Assert.Equal(0, result);
        }

        [Fact]
        public void CreateAvailableStock_NewEntry_AddsAndPersists()
        {
            var set = new TestDbSet<CatalogItemsStock>(new[]
            {
                new CatalogItemsStock { StockId = 3, CatalogItemId = 99, Date = new DateTime(2023, 1, 1), AvailableStock = 1 }
            });
            var model = CreateModel(stocks: set);
            var service = new CatalogService(model.Object);
            var newStock = new CatalogItemsStock { CatalogItemId = 1, Date = new DateTime(2024, 1, 15), AvailableStock = 10 };

            service.CreateAvailableStock(newStock);

            Assert.Equal(4, newStock.StockId);
            Assert.Contains(newStock, set.Items);
            model.Verify(m => m.SaveChanges(), Times.Once);
        }
    }
}
