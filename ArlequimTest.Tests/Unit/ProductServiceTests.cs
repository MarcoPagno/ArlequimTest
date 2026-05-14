using ArlequimTest.Api.DTOs;
using ArlequimTest.Api.Exceptions;
using ArlequimTest.Api.Models;
using ArlequimTest.Api.Services;

namespace ArlequimTest.Tests.Unit
{
    public class ProductServiceTests
    {
        private readonly ProductService _service = new ProductService();

        //Product Creation (POST) TESTS
        [Fact]
        public void Create_ShouldThrow_WhenNameAlreadyUsed()
        {
            var newProduct = new CreateProductDto
            {
                Name = "Unitusedproductnameerrortest1",
                Description = "product name error test Description",
                Price = 99.99m
            };

            _service.Create(newProduct);

            var dto = new CreateProductDto
            {
                Name = "UnitUsedProductNameErrorTest1",
                Description = "product name error test Description",
                Price = 100.99m
            };

            var exception = Assert.Throws<ValidationError>(() => _service.Create(dto));
            Assert.Equal("Product name already used", exception.Message);
        }

        [Fact]
        public void Create_ShouldThrow_WhenPriceFormatWrong()
        {
            var dto = new CreateProductDto
            {
                Name = "UnitPriceFormatErrorTest",
                Description = "product name error test Description",
                Price = 0.999m
            };

            var exception = Assert.Throws<ValidationError>(() => _service.Create(dto));
            Assert.Equal("Wrong price format", exception.Message);
        }

        [Fact]
        public void Create_ShouldReturnProduct_WhenDataIsValid()
        {
            var dto = new CreateProductDto
            {
                Name = "UnitCreateProductTest",
                Description = "Unit create product test Description",
                Price = 100.99m
            };

            var result = _service.Create(dto);

            Assert.Equal("UnitCreateProductTest", result.Name);
            Assert.Equal("Unit create product test Description", result.Description);
            Assert.Equal(100.99m, result.Price);
        }


        //Product Update (PATCH) TESTS
        [Fact]
        public void Update_ShouldThrow_WhenNameNotFound()
        {
            Assert.Throws<NotFoundError>(() => _service.UpdateByName("NotFound", new UpdateProductDto()));
        }

        [Fact]
        public void Update_ShouldReturnProduct_WhenDataIsValid()
        {
            var dto = new CreateProductDto
            {
                Name = "UnitUpdateProductTest",
                Description = "Unit update product test",
                Price = 5.00m
            };
            _service.Create(dto);


            var updatedDto = new UpdateProductDto
            {
                Description = "Updated product test",
                Price = 12.00m
            };
            _service.UpdateByName(dto.Name, updatedDto);

            var searchResult = _service.FindByName(dto.Name);

            Assert.Equal("UnitUpdateProductTest", searchResult.Name);
            Assert.Equal("Updated product test", searchResult.Description);
            Assert.Equal(12.00m, searchResult.Price);

            var updated2Dto = new UpdateProductDto
            {
                Name = "UpdatedProductTest"
            };
            _service.UpdateByName(dto.Name, updated2Dto);

            var searchResult2 = _service.FindByName(updated2Dto.Name);

            Assert.Equal("UpdatedProductTest", searchResult2.Name);
            Assert.Equal("Updated product test", searchResult2.Description);
            Assert.Equal(12.00m, searchResult2.Price);
        }


        //Product List (GET) TESTS
        [Fact]
        public void List_ShouldReturnProductList()
        {
            var dto = new CreateProductDto
            {
                Name = "UnitListProductTest2",
                Description = "Unit create product test 2",
                Price = 2.00m
            };
            var dto2 = new CreateProductDto
            {
                Name = "UnitListProductTest3",
                Description = "Unit create product test 3",
                Price = 3.00m
            };
            _service.Create(dto);
            _service.Create(dto2);

            var result = _service.List();

            Assert.True(result.Count > 1);

            Assert.Equal("UnitListProductTest3", result.Last().Name);
            Assert.Equal("Unit create product test 3", result.Last().Description);
            Assert.Equal(3.00m, result.Last().Price);
        }

        [Fact]
        public void Find_ShouldThrow_WhenNameNotFound()
        {
            var exception = Assert.Throws<NotFoundError>(() => _service.FindByName("UnitFindProductTest99"));
            Assert.Equal("Product not found", exception.Message);
        }

        [Fact]
        public void Find_ShouldReturnProduct_WhenDataIsValid()
        {
            var dto = new CreateProductDto
            {
                Name = "UnitFindProductTest",
                Description = "Unit find product test",
                Price = 99.00m
            };
            _service.Create(dto);

            var result = _service.FindByName(dto.Name);

            Assert.Equal("UnitFindProductTest", result.Name);
            Assert.Equal("Unit find product test", result.Description);
            Assert.Equal(99.00m, result.Price);
        }


        //Product DELETE (DELETE) TESTS
        [Fact]
        public void Delete_ShouldThrow_WhenNameNotFound()
        {
            Assert.Throws<NotFoundError>(() => _service.DeleteByName("NotFound"));
        }

        [Fact]
        public void Delete_ShouldReturnNoContent_WhenDataIsValid()
        {
            var dto = new CreateProductDto
            {
                Name = "DeletedLater",
                Description = "Unit delete product test Description",
                Price = 0.01m
            };
             _service.Create(dto);
            
            Assert.True(_service.DeleteByName(dto.Name));

            Assert.Throws<NotFoundError>(() => _service.FindByName(dto.Name));
        }

    }
}
