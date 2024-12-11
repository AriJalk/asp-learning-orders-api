using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.Core.Domain.RepositoryContracts;
using Orders.Core.DTO;
using Orders.Core.ServiceContracts.OrderItems;
using Orders.WebAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Tests.ControllerTests
{
	public class OrderItemsControllerTests
	{
		private readonly Mock<IOrderItemAdderService> _orderItemAdderServiceMock;
		private readonly Mock<IOrderItemGetterService> _orderItemGetterServiceMock;
		private readonly Mock<IOrderItemUpdaterService> _orderItemUpdaterServiceMock;
		private readonly Mock<IOrderItemDeleterService> _orderItemDeleterServiceMock;

		private readonly IFixture _fixture;

		private readonly OrderItemsController _controller;

		public OrderItemsControllerTests()
		{
			_fixture = new Fixture();

			_orderItemAdderServiceMock = new Mock<IOrderItemAdderService>();
			_orderItemGetterServiceMock = new Mock<IOrderItemGetterService>();
			_orderItemUpdaterServiceMock = new Mock<IOrderItemUpdaterService>();
			_orderItemDeleterServiceMock = new Mock<IOrderItemDeleterService>();

			_controller = new OrderItemsController(new Mock<ILogger<OrderItemsController>>().Object, _orderItemAdderServiceMock.Object, _orderItemGetterServiceMock.Object, _orderItemUpdaterServiceMock.Object, _orderItemDeleterServiceMock.Object);
		}

		#region GetOrderItemsByOrderId
		[Fact]
		public async Task GetOrderItemsByOrderId_MissingOrder_ReturnsEmptyList()
		{
			//Arrange
			_orderItemGetterServiceMock.Setup(temp => temp.GetOrderItemsByOrderId(It.IsAny<Guid>())).ReturnsAsync(new List<OrderItemResponse>());

			//Act
			ActionResult<List<OrderItemResponse>> actionResult = await _controller.GetOrderItemsByOrderId(Guid.Empty);

			//Assert
			OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
			List<OrderItemResponse> actualItemResponseList = Assert.IsAssignableFrom<List<OrderItemResponse>>(okResult.Value);

			actualItemResponseList.Should().BeEmpty();
		}

		[Fact]
		public async Task GetOrderItemsByOrderId_MatchingOrder_ReturnsMatchingList()
		{
			//Arrange
			List<OrderItemResponse> expectedResponses = _fixture.Create<List<OrderItemResponse>>();

			_orderItemGetterServiceMock.Setup(temp => temp.GetOrderItemsByOrderId(It.IsAny<Guid>())).ReturnsAsync(expectedResponses);

			//Act
			ActionResult<List<OrderItemResponse>> actionResult = await _controller.GetOrderItemsByOrderId(Guid.Empty);

			//Assert
			OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
			List<OrderItemResponse> actualItemResponseList = Assert.IsAssignableFrom<List<OrderItemResponse>>(okResult.Value);

			actualItemResponseList.Should().BeEquivalentTo(expectedResponses);

		}


		#endregion

		#region GetOrderItemByOrderItemId
		[Fact]
		public async Task GetOrderItemByOrderItemId_MissingItemId_NotFoundResult()
		{
			//Arrange
			Guid itemId = Guid.NewGuid();
			Guid orderId = Guid.NewGuid();
			_orderItemGetterServiceMock.Setup(temp => temp.GetOrderItemByOrderItemId(It.IsAny<Guid>())).ThrowsAsync(new KeyNotFoundException());

			//Act
			ActionResult<OrderItemResponse> actionResult = await _controller.GetOrderItemByOrderItemId(orderId, itemId);

			//Assert
			NotFoundObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
		}

		[Fact]
		public async Task GetOrderItemByOrderItemId_MismatchOrderId_NotFoundResult()
		{
			//Arrange
			Guid itemId = Guid.NewGuid();
			Guid itemOrderId = Guid.NewGuid();
			Guid orderId = Guid.NewGuid();

			OrderItemResponse orderItemResponse = _fixture.Build<OrderItemResponse>()
				.With(oi => oi.OrderItemId, itemId)
				.With(oi => oi.OrderId, itemOrderId)
				.Create();
			_orderItemGetterServiceMock.Setup(temp => temp.GetOrderItemByOrderItemId(It.Is<Guid>(g => g.Equals(itemId)))).ReturnsAsync(orderItemResponse);

			//Act
			ActionResult<OrderItemResponse> actionResult = await _controller.GetOrderItemByOrderItemId(orderId, itemId);

			//Assert
			NotFoundObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
		}

		[Fact]
		public async Task GetOrderItemByOrderItemId_ValidItemId_OkResultWithResponse()
		{
			//Arrange
			Guid itemId = Guid.NewGuid();
			Guid orderId = Guid.NewGuid();

			OrderItemResponse expectedOrderItemResponse = _fixture.Build<OrderItemResponse>()
				.With(oi => oi.OrderItemId, itemId)
				.With(oi => oi.OrderId, orderId)
				.Create();
			_orderItemGetterServiceMock.Setup(temp => temp.GetOrderItemByOrderItemId(It.Is<Guid>(g => g.Equals(itemId)))).ReturnsAsync(expectedOrderItemResponse);

			//Act
			ActionResult<OrderItemResponse> actionResult = await _controller.GetOrderItemByOrderItemId(orderId, itemId);

			//Assert
			OkObjectResult okObject = Assert.IsType<OkObjectResult>(actionResult.Result);
			OrderItemResponse actualOrderItemResponse = Assert.IsAssignableFrom<OrderItemResponse>(okObject.Value);
		}

		#endregion

		#region AddOrderItem
		[Fact]
		public async Task AddOrderItem_MismatchOrderId_ReturnsBadRequest()
		{
			//Arrange
			Guid orderId = Guid.Parse("6322CC06-E8AE-4DD6-9E8E-69A9E71206BE");
			Guid itemOrderId = Guid.Parse("C54884BD-8388-4D27-ADF0-068D6573FAB1");
			OrderItemAddRequest addRequest = _fixture.Build<OrderItemAddRequest>()
				.With(r => r.OrderId, itemOrderId)
				.Create();

			//Act
			ActionResult<OrderItemResponse> actionResult = await _controller.AddOrderItem(orderId, addRequest);

			//Assert
			BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
		}

		[Fact]
		public async Task AddOrderItem_MissingOrder_ReturnsNotFoundResult()
		{
			//Arrange
			Guid orderId = Guid.Parse("6322CC06-E8AE-4DD6-9E8E-69A9E71206BE");
			OrderItemAddRequest addRequest = _fixture.Build<OrderItemAddRequest>()
				.With(r => r.OrderId, orderId)
				.Create();

			_orderItemAdderServiceMock.Setup(temp => temp.AddOrderItem(It.IsAny<OrderItemAddRequest>())).ThrowsAsync(new KeyNotFoundException());

			//Act
			ActionResult<OrderItemResponse> actionResult = await _controller.AddOrderItem(orderId, addRequest);

			//Assert
			NotFoundObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
		}

		[Fact]
		public async Task AddOrderItem_ValidAddRequest_ReturnsCreatedAtActionResultWithResponse()
		{
			//Arrange
			Guid orderId = Guid.Parse("6322CC06-E8AE-4DD6-9E8E-69A9E71206BE");
			OrderItemAddRequest addRequest = _fixture.Build<OrderItemAddRequest>()
				.With(r => r.OrderId, orderId)
				.Create();
			OrderItemResponse expectedOrderItemResponse = addRequest.ToOrderItem().ToResponse();

			_orderItemAdderServiceMock.Setup(temp => temp.AddOrderItem(It.Is<OrderItemAddRequest>(r => r.Equals(addRequest)))).ReturnsAsync(expectedOrderItemResponse);

			//Act
			ActionResult<OrderItemResponse> actionResult = await _controller.AddOrderItem(orderId, addRequest);

			//Assert
			CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
			OrderItemResponse actualItemResponse = Assert.IsAssignableFrom<OrderItemResponse>(createdResult.Value);
			actualItemResponse.Should().BeEquivalentTo(expectedOrderItemResponse);
		}

		#endregion

		#region DeleteOrderItem
		[Fact]
		public async Task DeleteOrderItem_MismatchOrderId_ReturnsBadRequest()
		{
			//Arrange
			Guid orderId = Guid.Parse("6322CC06-E8AE-4DD6-9E8E-69A9E71206BE");
			Guid itemOrderId = Guid.Parse("C54884BD-8388-4D27-ADF0-068D6573FAB1");
			Guid itemId = Guid.Parse("298B51C3-B97F-4575-8449-541EB2DF2AB8");
			OrderItemResponse itemResponse = _fixture.Build<OrderItemResponse>()
				.With(r => r.OrderId, itemOrderId)
				.With(r => r.OrderItemId, itemId)
				.Create();
			_orderItemGetterServiceMock.Setup(temp => temp.GetOrderItemByOrderItemId(It.Is<Guid>(g => g.Equals(itemId)))).ReturnsAsync(itemResponse);

			//Act
			IActionResult actionResult = await _controller.DeleteOrderItem(orderId, itemId);

			//Assert
			BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
		}

		[Fact]
		public async Task DeleteOrderItem_MissingItem_ReturnsNotFoundResult()
		{
			//Arrange
			Guid orderId = Guid.Parse("6322CC06-E8AE-4DD6-9E8E-69A9E71206BE");
			Guid itemId = Guid.Parse("298B51C3-B97F-4575-8449-541EB2DF2AB8");
			_orderItemGetterServiceMock.Setup(temp => temp.GetOrderItemByOrderItemId(It.Is<Guid>(g => g.Equals(itemId)))).ThrowsAsync(new KeyNotFoundException());

			//Act
			IActionResult actionResult = await _controller.DeleteOrderItem(orderId, itemId);

			//Assert
			NotFoundObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult);
		}

		[Fact]
		public async Task DeleteOrderItem_DeleteFailure_ThrowsApplicationException()
		{
			//Arrange
			Guid orderId = Guid.Parse("6322CC06-E8AE-4DD6-9E8E-69A9E71206BE");
			Guid itemId = Guid.Parse("298B51C3-B97F-4575-8449-541EB2DF2AB8");

			OrderItemResponse itemResponse = _fixture.Build<OrderItemResponse>()
				.With(oi => oi.OrderId, orderId)
				.With(oi => oi.OrderItemId, itemId)
				.Create();

			_orderItemGetterServiceMock.Setup(temp => temp.GetOrderItemByOrderItemId(It.Is<Guid>(g => g.Equals(itemId)))).ReturnsAsync(itemResponse);
			_orderItemDeleterServiceMock.Setup(temp => temp.DeleteOrderItemByOrderItemId(It.Is<Guid>(g => g.Equals(itemId)))).ReturnsAsync(false);

			//Act
			Func<Task> action = async () =>
			{
				await _controller.DeleteOrderItem(orderId, itemId);
			};

			//Assert
			await action.Should().ThrowAsync<ApplicationException>();
		}

		[Fact]
		public async Task DeleteOrderItem_DeleteSuccessful_ReturnsNoContentResult()
		{
			//Arrange
			Guid orderId = Guid.Parse("6322CC06-E8AE-4DD6-9E8E-69A9E71206BE");
			Guid itemId = Guid.Parse("298B51C3-B97F-4575-8449-541EB2DF2AB8");

			OrderItemResponse itemResponse = _fixture.Build<OrderItemResponse>()
				.With(oi => oi.OrderId, orderId)
				.With(oi => oi.OrderItemId, itemId)
				.Create();

			_orderItemGetterServiceMock.Setup(temp => temp.GetOrderItemByOrderItemId(It.Is<Guid>(g => g.Equals(itemId)))).ReturnsAsync(itemResponse);
			_orderItemDeleterServiceMock.Setup(temp => temp.DeleteOrderItemByOrderItemId(It.Is<Guid>(g => g.Equals(itemId)))).ReturnsAsync(true);

			//Act
			IActionResult actionResult = await _controller.DeleteOrderItem(orderId, itemId);

			//Assert
			Assert.IsType<NoContentResult>(actionResult);
		}

		#endregion

		#region UpdateOrderItem
		

		[Fact]
		public async Task UpdateOrderItem_MismatchOrderItemId_ReturnsBadRequest()
		{
			//Arrange
			Guid orderId = Guid.Parse("6322CC06-E8AE-4DD6-9E8E-69A9E71206BE");
			Guid itemOrderId = Guid.Parse("C54884BD-8388-4D27-ADF0-068D6573FAB1");
			Guid itemId = Guid.Parse("298B51C3-B97F-4575-8449-541EB2DF2AB8");
			Guid otherItemId = Guid.Parse("F6A509D0-9ADF-4AED-838E-5E8ED1CB994A");
			OrderItemUpdateRequest updateRequest = _fixture.Build<OrderItemUpdateRequest>()
				.With(r => r.OrderId, orderId)
				.With(r => r.OrderItemId, itemId)
				.Create();

			//Act
			ActionResult<OrderItemResponse> actionResult = await _controller.UpdateOrderItem(orderId, otherItemId, updateRequest);

			//Assert
			BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
		}

		[Fact]
		public async Task UpdateOrderItem_MismatchOrderId_ReturnsBadRequest()
		{
			//Arrange
			Guid orderId = Guid.Parse("6322CC06-E8AE-4DD6-9E8E-69A9E71206BE");
			Guid itemOrderId = Guid.Parse("C54884BD-8388-4D27-ADF0-068D6573FAB1");
			Guid itemId = Guid.Parse("298B51C3-B97F-4575-8449-541EB2DF2AB8");
			OrderItemUpdateRequest updateRequest = _fixture.Build<OrderItemUpdateRequest>()
				.With(r => r.OrderId, itemOrderId)
				.With(r => r.OrderItemId, itemId)
				.Create();

			//Act
			ActionResult<OrderItemResponse> actionResult = await _controller.UpdateOrderItem(orderId, itemId, updateRequest);

			//Assert
			BadRequestObjectResult badRequest = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
		}

		[Fact]
		public async Task UpdateOrderItem_ItemOrOrderNotFound_ReturnsNotFoundResult()
		{
			//Arrange
			Guid orderId = Guid.Parse("6322CC06-E8AE-4DD6-9E8E-69A9E71206BE");
			Guid itemId = Guid.Parse("298B51C3-B97F-4575-8449-541EB2DF2AB8");
			OrderItemUpdateRequest updateRequest = _fixture.Build<OrderItemUpdateRequest>()
				.With(r => r.OrderId, orderId)
				.With(r => r.OrderItemId, itemId)
				.Create();

			_orderItemUpdaterServiceMock.Setup(temp => temp.UpdateOrderItem(It.Is<OrderItemUpdateRequest>(oi => oi.Equals(updateRequest)))).ThrowsAsync(new KeyNotFoundException());

			//Act
			ActionResult<OrderItemResponse> actionResult = await _controller.UpdateOrderItem(orderId, itemId, updateRequest);

			//Assert
			NotFoundObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
		}

		[Fact]
		public async Task UpdateOrderItem_UpdateSuccessful_ReturnsOkResultWithItemResponse()
		{
			//Arrange
			Guid orderId = Guid.Parse("6322CC06-E8AE-4DD6-9E8E-69A9E71206BE");
			Guid itemId = Guid.Parse("298B51C3-B97F-4575-8449-541EB2DF2AB8");
			OrderItemUpdateRequest updateRequest = _fixture.Build<OrderItemUpdateRequest>()
				.With(r => r.OrderId, orderId)
				.With(r => r.OrderItemId, itemId)
				.Create();

			OrderItemResponse expectedItemResponse = updateRequest.ToOrderItem().ToResponse();

			_orderItemUpdaterServiceMock.Setup(temp => temp.UpdateOrderItem(It.Is<OrderItemUpdateRequest>(oi => oi.Equals(updateRequest)))).ReturnsAsync(expectedItemResponse);

			//Act
			ActionResult<OrderItemResponse> actionResult = await _controller.UpdateOrderItem(orderId, itemId, updateRequest);

			//Assert
			OkObjectResult okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
			OrderItemResponse actualItemResponse = Assert.IsAssignableFrom<OrderItemResponse>(okResult.Value);
			actualItemResponse.Should().BeEquivalentTo(expectedItemResponse);
		}

		#endregion
	}


}
