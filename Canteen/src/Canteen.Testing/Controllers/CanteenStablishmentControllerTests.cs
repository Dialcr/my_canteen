
using Canteen.Controllers;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto;
using Canteen.Services.Dto.Establishment;
using Canteen.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OneOf;

namespace Canteen.Testing.Controllers
{
    public class CanteenStablishmentControllerTests
    {
        private readonly Mock<IEstablishmentService> _establishmentServiceMock;
        private readonly Mock<ILogger<CanteenStablishmentController>> _loggerMock;
        private readonly CanteenStablishmentController _controller;

        public CanteenStablishmentControllerTests()
        {
            _establishmentServiceMock = new Mock<IEstablishmentService>();
            _loggerMock = new Mock<ILogger<CanteenStablishmentController>>();
            _controller = new CanteenStablishmentController(_establishmentServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void GetAllSEstablishments_ReturnsOkResult_WithEstablishments()
        {
            // Arrange
            int page = 1;
            int perPage = 10;

            var expectedEstablishments = new PagedResponse<EstablishmentOutputDto>(new List<EstablishmentOutputDto>
                {
                    new() { Id = 1, Name = "Test Establishment 1" },
                    new() { Id = 2, Name = "Test Establishment 2" },
                }, page, perPage, 1, 2);

            _establishmentServiceMock.Setup(x => x.GetAllEstablishments(page, perPage))
                .Returns(expectedEstablishments);

            // Act
            var result = _controller.GetAllSEstablishments(page, perPage);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedEstablishments = Assert.IsType<PagedResponse<EstablishmentOutputDto>>(okResult.Value);
            Assert.Equal(expectedEstablishments, returnedEstablishments);
        }

        [Fact]
        public async Task GetEstablishmentById_ReturnsOkResult_WhenEstablishmentExists()
        {
            // Arrange
            var establishmentId = 1;
            var expectedEstablishment = new EstablishmentOutputDto { Id = establishmentId, Name = "Test Establishment" };

            _establishmentServiceMock.Setup(x => x.GetEstablishmentByIdAsync(establishmentId))
                .ReturnsAsync(OneOf<ResponseErrorDto, EstablishmentOutputDto>.FromT1(expectedEstablishment));

            // Act
            var result = await _controller.GetEstablishmentById(establishmentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedEstablishment = Assert.IsType<EstablishmentOutputDto>(okResult.Value);
            Assert.Equal(expectedEstablishment, returnedEstablishment);
        }

        [Fact]
        public async Task GetEstablishmentById_ReturnsNotFound_WhenEstablishmentDoesNotExist()
        {
            // Arrange
            var establishmentId = 999;
            var error = new ResponseErrorDto { Detail = "Establishment not found" };

            _establishmentServiceMock.Setup(x => x.GetEstablishmentByIdAsync(establishmentId))
                .ReturnsAsync(OneOf<ResponseErrorDto, EstablishmentOutputDto>.FromT0(error));

            // Act
            var result = await _controller.GetEstablishmentById(establishmentId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
