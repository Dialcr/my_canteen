using Canteen.Controllers;
using Canteen.DataAccess.Entities;
using Canteen.DataAccess.Enums;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OneOf;

namespace Canteen.Testing.Controllers
{
    public class CanteenProductControllerTests
    {
        private readonly Mock<IProductServices> _productServicesMock;
        private readonly Mock<ILogger<CanteenProductController>> _loggerMock;
        private readonly CanteenProductController _controller;

        public CanteenProductControllerTests()
        {
            _productServicesMock = new Mock<IProductServices>();
            _loggerMock = new Mock<ILogger<CanteenProductController>>();
            _controller = new CanteenProductController(_productServicesMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void GetCanteenProductById_ReturnsOkResult_WhenProductExists()
        {
            // Arrange
            var productId = 1;
            var product = new Product { Id = productId, Name = "Test Product" };
            var expectedOutput = new ProductOutputDto { Id = productId, Name = "Test Product" };

            _productServicesMock.Setup(x => x.GetCantneeProductById(productId))
                .Returns(OneOf<ResponseErrorDto, Product>.FromT1(product));

            // Act
            var result = _controller.GetCanteenProductById(productId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProduct = Assert.IsType<ProductOutputDto>(okResult.Value);
            Assert.Equal(expectedOutput.Id, returnedProduct.Id);
        }

        [Fact]
        public void GetCanteenProductById_ReturnsBadRequest_WhenProductNotFound()
        {
            // Arrange
            var productId = 999;
            var error = new ResponseErrorDto { Status = 400, Detail = "Product not found" };

            _productServicesMock.Setup(x => x.GetCantneeProductById(productId))
                .Returns(OneOf<ResponseErrorDto, Product>.FromT0(error));

            // Act
            var result = _controller.GetCanteenProductById(productId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnedError = Assert.IsType<ResponseErrorDto>(badRequestResult.Value);
            Assert.Equal(400, returnedError.Status);
        }

        [Fact]
        public void GetCantneeProductsByCategory_ReturnsOkResult_WhenProductsExist()
        {
            // Arrange
            var category = ProductCategory.Entrante;
            var products = new List<ProductOutputDto>
            {
                new() { Id = 1, Name = "Product 1", Category = category },
                new() { Id = 2, Name = "Product 2", Category = category }
            };

            _productServicesMock.Setup(x => x.GetCantneeProductsByCategoryAsync(category))
                .ReturnsAsync(OneOf<ResponseErrorDto, ICollection<ProductOutputDto>>.FromT1(products));

            // Act
            var result = _controller.GetCantneeProductsByCategory(category);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProducts = Assert.IsAssignableFrom<ICollection<ProductOutputDto>>(okResult.Value);
            Assert.Equal(2, returnedProducts.Count);
        }

        [Fact]
        public void GetCantneeProductsByDietaryRestrictions_ReturnsOkResult_WhenProductsExist()
        {
            // Arrange
            var restriction = "Vegetarian";
            var products = new List<ProductOutputDto>
            {
                new() { Id = 1, Name = "Veg Product 1" },
                new() { Id = 2, Name = "Veg Product 2" }
            };

            _productServicesMock.Setup(x => x.GetCantneeProductsByDietaryRestrictions(restriction))
                .Returns(OneOf<ResponseErrorDto, ICollection<ProductOutputDto>>.FromT1(products));

            // Act
            var result = _controller.GetCantneeProductsByDietaryRestrictions(restriction);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProducts = Assert.IsAssignableFrom<ICollection<ProductOutputDto>>(okResult.Value);
            Assert.Equal(2, returnedProducts.Count);
        }

        [Fact]
        public void GetCantneeProductsByDietaryRestrictions_ReturnsBadRequest_WhenNoProductsFound()
        {
            // Arrange
            var restriction = "NonExistent";
            var error = new ResponseErrorDto { Status = 400, Detail = "No products found" };

            _productServicesMock.Setup(x => x.GetCantneeProductsByDietaryRestrictions(restriction))
                .Returns(OneOf<ResponseErrorDto, ICollection<ProductOutputDto>>.FromT0(error));

            // Act
            var result = _controller.GetCantneeProductsByDietaryRestrictions(restriction);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
