using AutoFixture;
using Castle.Core.Logging;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.Core.DTO;
using Orders.Core.ServiceContracts.Orders;
using Orders.WebAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Tests.ControllerTests
{
	public class OrdersControllerTests
	{
		private readonly OrdersController _controller;

		private readonly Mock<IOrderAdderService> _orderAdderServiceMock;

		private readonly Mock<IOrderGetterService> _orderGetterServiceMock;

		private readonly Mock<IOrderFilterService> _orderFilterServiceMock;

		private readonly Mock<IOrderUpdaterService> _orderUpdaterServiceMock;

		private readonly Mock<IOrderDeleterService> _orderDeleterServiceMock;

		private readonly IFixture _fixture;

		public OrdersControllerTests()
		{
			_orderAdderServiceMock = new Mock<IOrderAdderService>();

			_orderGetterServiceMock = new Mock<IOrderGetterService>();

			_orderFilterServiceMock = new Mock<IOrderFilterService>();

			_orderUpdaterServiceMock = new Mock<IOrderUpdaterService>();

			_orderDeleterServiceMock = new Mock<IOrderDeleterService>();

			_fixture = new Fixture();

			_controller = new OrdersController(new Mock<ILogger<OrdersController>>().Object, _orderAdderServiceMock.Object, _orderGetterServiceMock.Object, _orderFilterServiceMock.Object, _orderUpdaterServiceMock.Object, _orderDeleterServiceMock.Object);
		}

		#region GetOrders
		[Fact]
		public async Task GetOrders_EmptyOrders_ReturnsEmptyList()
		{
			//Arrange
			_orderGetterServiceMock.Setup(temp => temp.GetAllOrders()).ReturnsAsync(new List<OrderResponse>());

			//Act
			ActionResult<IEnumerable<OrderResponse>> getResult = await _controller.GetOrders();

			//Assert
			OkObjectResult okResult = Assert.IsType<OkObjectResult>(getResult.Result);

			IEnumerable<OrderResponse> responseList = Assert.IsAssignableFrom<IEnumerable<OrderResponse>>(okResult.Value);

			responseList.Should().BeEmpty();

		}

		[Fact]
		public async Task GetOrders_SomeOrders_ReturnsOrderResponses()
		{
			//Arrange
			List<OrderResponse> expectedOrderResponses = _fixture.Create<List<OrderResponse>>();
			_orderGetterServiceMock.Setup(temp => temp.GetAllOrders()).ReturnsAsync(expectedOrderResponses);

			//Act
			ActionResult<IEnumerable<OrderResponse>> getResult = await _controller.GetOrders();

			//Assert
			OkObjectResult okResult = Assert.IsType<OkObjectResult>(getResult.Result);

			IEnumerable<OrderResponse> actualResponseListFromGet = Assert.IsAssignableFrom<IEnumerable<OrderResponse>>(okResult.Value);

			actualResponseListFromGet.Should().BeEquivalentTo(expectedOrderResponses);

		}

		#endregion

		#region GetOrderById
		[Fact]
		public async Task GetOrderById_CantFindOrder_ReturnNotFoundResult()
		{
			//Arrange
			string errorMessage = "Test Error message";
			_orderGetterServiceMock.Setup(temp => temp.GetOrderByOrderId(It.IsAny<Guid>())).ThrowsAsync(new KeyNotFoundException(errorMessage));
			Guid orderId = Guid.Parse("33FBD7AE-B370-426D-8161-87943D393E6F");

			//Act
			ActionResult<OrderResponse> resultFromGet = await _controller.GetOrderById(orderId);

			//Assert
			NotFoundObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(resultFromGet.Result);

			notFoundResult.Value.Should().BeEquivalentTo(errorMessage);
		}

		[Fact]
		public async Task GetOrderById_MatchingOrder_ReturnOkResultWithOrderResponse()
		{
			//Arrange
			Guid orderId = Guid.Parse("33FBD7AE-B370-426D-8161-87943D393E6F");
			OrderResponse expectedResponse = _fixture.Build<OrderResponse>()
				.With(r => r.OrderId, orderId)
				.Create();
			_orderGetterServiceMock.Setup(temp => temp.GetOrderByOrderId(It.Is<Guid>(g => g.Equals(orderId)))).ReturnsAsync(expectedResponse);

			//Act
			ActionResult<OrderResponse> responseFromGet = await _controller.GetOrderById(orderId);

			//Assert
			OkObjectResult okObjectResult = Assert.IsType<OkObjectResult>(responseFromGet.Result);
			OrderResponse actualOrderResponse = Assert.IsAssignableFrom<OrderResponse>(okObjectResult.Value);

			actualOrderResponse.Should().BeEquivalentTo(expectedResponse);
			okObjectResult.StatusCode.Should().Be(StatusCodes.Status200OK);
		}

		#endregion

		#region AddOrder

		#endregion

		#region UpdateOrder

		#endregion

		#region DeleteOrder

		#endregion
	}
}
