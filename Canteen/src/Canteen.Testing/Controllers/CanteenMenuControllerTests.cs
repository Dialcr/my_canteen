using Canteen.Controllers;
using Canteen.DataAccess.Entities;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto;
using Canteen.Services.Dto.Menu;
using Canteen.Services.Dto.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OneOf;

namespace Canteen.Testing.Controllers
{
    public class CanteenMenuControllerTests
    {
        private readonly Mock<IMenuServices> _menuServicesMock;
        private readonly Mock<ILogger<CanteenMenuController>> _loggerMock;
        private readonly CanteenMenuController _controller;

        public CanteenMenuControllerTests()
        {
            _menuServicesMock = new Mock<IMenuServices>();
            _loggerMock = new Mock<ILogger<CanteenMenuController>>();
            _controller = new CanteenMenuController(_menuServicesMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void GetMenuByEstablishmentDate_ReturnsOkResult_WhenMenuExists()
        {
            // Arrange
            var establishmentId = 1;
            var date = DateTime.Now;
            var menu = new Menu { Id = 1, EstablishmentId = establishmentId };
            var expectedMenuOutput = new MenuOutputDto { Id = 1 };

            _menuServicesMock.Setup(x => x.GetMenuByEstablishmentAndDate(establishmentId, date))
                .Returns(OneOf<ResponseErrorDto, Menu>.FromT1(menu));

            // Act
            var result = _controller.GetMenuByEstablishmentDate(establishmentId, date);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMenu = Assert.IsType<MenuOutputDto>(okResult.Value);
            Assert.Equal(expectedMenuOutput.Id, returnedMenu.Id);
        }

        [Fact]
        public void GetMenuByEstablishmentDate_ReturnsBadRequest_WhenMenuNotFound()
        {
            // Arrange
            var establishmentId = 1;
            var date = DateTime.Now;
            var error = new ResponseErrorDto
            {
                Status = 400,
                Detail = "Menu not found"
            };

            _menuServicesMock.Setup(x => x.GetMenuByEstablishmentAndDate(establishmentId, date))
                .Returns(OneOf<ResponseErrorDto, Menu>.FromT0(error));

            // Act
            var result = _controller.GetMenuByEstablishmentDate(establishmentId, date);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnedError = Assert.IsType<ResponseErrorDto>(badRequestResult.Value);
            Assert.Equal(400, returnedError.Status);
            Assert.Equal("Menu not found", returnedError.Detail);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void GetMenuByEstablishmentDate_ReturnsBadRequest_WhenInvalidEstablishmentId(int invalidId)
        {
            // Arrange
            var date = DateTime.Now;
            var error = new ResponseErrorDto
            {
                Status = 400,
                Detail = "Invalid establishment id"
            };

            _menuServicesMock.Setup(x => x.GetMenuByEstablishmentAndDate(invalidId, date))
                .Returns(OneOf<ResponseErrorDto, Menu>.FromT0(error));

            // Act
            var result = _controller.GetMenuByEstablishmentDate(invalidId, date);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CreateMenu_ReturnsOkResult_WhenMenuCreatedSuccessfully()
        {
            // Arrange
            var createMenuDto = new CreateMenuInputDto();
            var response = new Response<NoContentData>();

            _menuServicesMock.Setup(x => x.CreateMenu(createMenuDto))
                .Returns(OneOf<ResponseErrorDto, Response<NoContentData>>.FromT1(response));

            // Act
            var result = _controller.CreateMenu(createMenuDto);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void CreateMenu_ReturnsBadRequest_WhenCreationFails()
        {
            // Arrange
            var createMenuDto = new CreateMenuInputDto();
            var error = new ResponseErrorDto
            {
                Status = 400,
                Detail = "Failed to create menu"
            };

            _menuServicesMock.Setup(x => x.CreateMenu(createMenuDto))
                .Returns(OneOf<ResponseErrorDto, Response<NoContentData>>.FromT0(error));

            // Act
            var result = _controller.CreateMenu(createMenuDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnedError = Assert.IsType<ResponseErrorDto>(badRequestResult.Value);
            Assert.Equal(400, returnedError.Status);
            Assert.Equal("Failed to create menu", returnedError.Detail);
        }
    }
}