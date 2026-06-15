using System;
using System.Collections.Generic;
using System.Linq;
using eShopModernizedMVC.Models;
using eShopModernizedMVC.ViewModel;
using Xunit;

namespace eShopModernizedMVCTests.ViewModel
{
    public class PaginatedItemsViewModelTests
    {
        [Fact]
        public void Constructor_SetsAllProperties()
        {
            int pageIndex = 2;
            int pageSize = 10;
            long count = 100;
            var data = new List<CatalogItem>
            {
                new CatalogItem { Id = 1, Name = "Item 1" },
                new CatalogItem { Id = 2, Name = "Item 2" }
            };

            var viewModel = new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, count, data);

            Assert.Equal(pageIndex, viewModel.ActualPage);
            Assert.Equal(pageSize, viewModel.ItemsPerPage);
            Assert.Equal(count, viewModel.TotalItems);
            Assert.Equal(data, viewModel.Data);
        }

        [Fact]
        public void Constructor_ComputesTotalPages()
        {
            var viewModel = new PaginatedItemsViewModel<CatalogItem>(0, 10, 25, new List<CatalogItem>());

            Assert.Equal(3, viewModel.TotalPages);
        }

        [Fact]
        public void Constructor_ExactMultiple_ComputesTotalPages()
        {
            var viewModel = new PaginatedItemsViewModel<CatalogItem>(0, 10, 20, new List<CatalogItem>());

            Assert.Equal(2, viewModel.TotalPages);
        }

        [Fact]
        public void Constructor_EmptyData_SetsEmptyData()
        {
            var data = new List<CatalogItem>();

            var viewModel = new PaginatedItemsViewModel<CatalogItem>(0, 10, 0, data);

            Assert.Empty(viewModel.Data);
            Assert.Equal(0, viewModel.TotalItems);
            Assert.Equal(0, viewModel.TotalPages);
        }

        [Fact]
        public void Constructor_NullData_SetsNullData()
        {
            var viewModel = new PaginatedItemsViewModel<CatalogItem>(0, 10, 0, null);

            Assert.Null(viewModel.Data);
        }

        [Fact]
        public void Constructor_NegativePageIndex_HandlesCorrectly()
        {
            var viewModel = new PaginatedItemsViewModel<CatalogItem>(-1, 10, 100, new List<CatalogItem>());

            Assert.Equal(-1, viewModel.ActualPage);
        }

        [Fact]
        public void Constructor_ZeroPageSize_ThrowsDivideByZero()
        {
            Assert.Throws<DivideByZeroException>(
                () => new PaginatedItemsViewModel<CatalogItem>(0, 0, 100, new List<CatalogItem>()));
        }

        [Fact]
        public void Data_ReturnsCorrectItems()
        {
            var items = new List<CatalogItem>
            {
                new CatalogItem { Id = 1, Name = "Item 1" },
                new CatalogItem { Id = 2, Name = "Item 2" },
                new CatalogItem { Id = 3, Name = "Item 3" }
            };
            var viewModel = new PaginatedItemsViewModel<CatalogItem>(0, 10, 3, items);

            var result = viewModel.Data.ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal(2, result[1].Id);
            Assert.Equal(3, result[2].Id);
        }

        [Fact]
        public void ActualPage_IsSetCorrectly()
        {
            var viewModel = new PaginatedItemsViewModel<CatalogItem>(5, 10, 100, null);

            Assert.Equal(5, viewModel.ActualPage);
        }

        [Fact]
        public void ItemsPerPage_IsSetCorrectly()
        {
            var viewModel = new PaginatedItemsViewModel<CatalogItem>(0, 25, 100, null);

            Assert.Equal(25, viewModel.ItemsPerPage);
        }

        [Fact]
        public void TotalItems_IsSetCorrectly()
        {
            var viewModel = new PaginatedItemsViewModel<CatalogItem>(0, 10, 500, null);

            Assert.Equal(500, viewModel.TotalItems);
        }

        [Fact]
        public void GenericType_WorksDifferentTypes()
        {
            var stringData = new List<string> { "Item1", "Item2", "Item3" };

            var viewModel = new PaginatedItemsViewModel<string>(0, 10, 3, stringData);

            Assert.Equal(3, viewModel.Data.Count());
            Assert.Equal("Item1", viewModel.Data.First());
        }
    }
}
