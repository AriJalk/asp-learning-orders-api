using AutoFixture;
using Castle.Core.Logging;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.Core.Domain.Entities;
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
		[Fact]
		public async Task AddOrder_AddingFailed_ThrowsException()
		{
			//Arrange
			OrderAddRequest request = _fixture.Create<OrderAddRequest>();
			_orderAdderServiceMock.Setup(temp => temp.AddOrder(It.IsAny<OrderAddRequest>())).ThrowsAsync(new Exception());

			//Act
			Func<Task> action = async () =>
			{
				await _controller.AddOrder(request);
			};

			//Assert
			await action.Should().ThrowAsync<Exception>();
		}

		[Fact]
		public async Task AddOrder_AddingSuccessful_ReturnsCreatedAtAction()
		{
			//Arrange
			OrderAddRequest addRequest = _fixture.Create<OrderAddRequest>();
			Order order = _fixture.Build<Order>()
				.With(o => o.CustomerName, addRequest.CustomerName)
				.Create();
			OrderResponse expectedOrderResponse = order.ToOrderResponse();
			expectedOrderResponse.OrderItems = addRequest.OrderItems.Select(oi => oi.ToOrderItem().ToResponse()).ToList();
			_orderAdderServiceMock.Setup(temp => temp.AddOrder(It.Is<OrderAddRequest>(r => r.Equals(addRequest)))).ReturnsAsync(expectedOrderResponse);

			//Act
			ActionResult<OrderResponse> actionResult = await _controller.AddOrder(addRequest);

			//Assert
			CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
			OrderResponse actualOrderResponse = Assert.IsAssignableFrom<OrderResponse>(createdResult.Value);
			actualOrderResponse.Should().BeEquivalentTo(expectedOrderResponse);
		}


		#endregion

		#region UpdateOrder
		[Fact]
		public async Task UpdateOrder_MismatchId_ReturnsBadRequest()
		{
			//Arrange
			Guid orderId = Guid.Parse("C85D2C71-CCF4-4C4E-A63F-FFF249E24884");
			Guid updateOrderId = Guid.Parse("94AA5313-F94F-4887-A8CF-B1C836FFFEBA");
			OrderUpdateRequest updateRequest = _fixture.Build<OrderUpdateRequest>()
				.With(r => r.OrderId, updateOrderId)
				.Create();

			//Act
			ActionResult<OrderResponse> actionResult = await _controller.UpdateOrder(orderId, updateRequest);

			//Assert
			BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
		}

		[Fact]
		public async Task UpdateOrder_UpdateFailure_ThrowsException()
		{
			//Arrange
			Guid orderId = Guid.Parse("C85D2C71-CCF4-4C4E-A63F-FFF249E24884");
			OrderUpdateRequest updateRequest = _fixture.Build<OrderUpdateRequest>()
				.With(r => r.OrderId, orderId)
				.Create();
			_orderUpdaterServiceMock.Setup(temp => temp.UpdateOrder(It.Is<OrderUpdateRequest>(r => r.Equals(updateRequest)))).ThrowsAsync(new Exception());

			//Act
			Func<Task> action = async () =>
			{
				await _controller.UpdateOrder(orderId, updateRequest);
			};

			//Assert
			await action.Should().ThrowAsync<Exception>();

		}

		[Fact]
		public async Task UpdateOrder_UpdateSuccessful_ReturnsOkResultWithOrderResponse()
		{
			//Arrange
			OrderUpdateRequest updateRequest = _fixture.Create<OrderUpdateRequest>();
			Order order = new Order() 
			{
				CustomerName = updateRequest.CustomerName,
				OrderDate = updateRequest.OrderDate,
				OrderId = updateRequest.OrderId,
				OrderNumber = updateRequest.OrderNumber,
				TotalAmount = updateRequest.TotalAmount,
			};
			OrderResponse expectedOrderResponse = order.ToOrderResponse();

			_orderUpdaterServiceMock.Setup(temp => temp.UpdateOrder(It.Is<OrderUpdateRequest>(r => r.Equals(updateRequest)))).ReturnsAsync(expectedOrderResponse);

			//Act
			ActionResult<OrderResponse> actionResult = await _controller.UpdateOrder(updateRequest.OrderId, updateRequest);

			//Assert
			OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
			OrderResponse actualOrderResponse = Assert.IsAssignableFrom<OrderResponse>(okResult.Value);
		}

		#endregion

		#region DeleteOrder

		[Fact]
		public async Task DeleteOrder_OrderNotFound_ReturnsNotFoundResult()
		{
			//Arrange
			Guid id = Guid.NewGuid();
			_orderDeleterServiceMock.Setup(temp => temp.DeleteOrderByOrderId(It.IsAny<Guid>())).ReturnsAsync(false);

			//Act
			IActionResult actionResult = await _controller.DeleteOrder(id);

			//Assert
			NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(actionResult);
		}

		[Fact]
		public async Task DeleteOrder_DeleteSuccessful_ReturnsNoContentResult()
		{
			//Arrange
			Guid id = Guid.NewGuid();
			_orderDeleterServiceMock.Setup(temp => temp.DeleteOrderByOrderId(It.IsAny<Guid>())).ReturnsAsync(true);

			//Act
			IActionResult actionResult = await _controller.DeleteOrder(id);

			//Assert
			NoContentResult noContentResult = Assert.IsType<NoContentResult>(actionResult);
		}

		#endregion
	}
}
