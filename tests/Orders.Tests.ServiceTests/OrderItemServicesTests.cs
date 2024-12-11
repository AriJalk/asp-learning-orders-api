using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.Core.Domain.Entities;
using Orders.Core.Domain.RepositoryContracts;
using Orders.Core.DTO;
using Orders.Core.ServiceContracts.OrderItems;
using Orders.Core.Services.OrderItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Tests.ServiceTests
{
	public class OrderItemServicesTests
	{
		private readonly IOrderItemAdderService _orderItemAdderService;
		private readonly IOrderItemDeleterService _orderItemDeleterService;
		private readonly IOrderItemGetterService _orderItemGetterService;
		private readonly IOrderItemUpdaterService _orderItemUpdaterService;

		private readonly Mock<IUnitOfWork> _unitOfWorkMock;
		private readonly IUnitOfWork _unitOfWork;

		private readonly IFixture _fixture;

		public OrderItemServicesTests()
		{
			_unitOfWorkMock = new Mock<IUnitOfWork>();
			_unitOfWork = _unitOfWorkMock.Object;

			_orderItemAdderService = new OrderItemAdderService(new Mock<ILogger<OrderItemAdderService>>().Object, _unitOfWork);
			_orderItemDeleterService = new OrderItemDeleterService(new Mock<ILogger<OrderItemDeleterService>>().Object, _unitOfWork);
			_orderItemGetterService = new OrderItemGetterService(new Mock<ILogger<OrderItemGetterService>>().Object, _unitOfWork);
			_orderItemUpdaterService = new OrderItemUpdaterService(new Mock<ILogger<OrderItemUpdaterService>>().Object, _unitOfWork);

			_fixture = new Fixture();
		}

		#region OrderItemAdderService
		[Fact]
		public async Task OrderItemAdderService_NullAddRequest_ThrowsArgumentNullException()
		{
			//Arrange
			OrderItemAddRequest? request = null;

			//Act
			Func<Task> action = async () =>
			{
				await _orderItemAdderService.AddOrderItem(request);
			};

			//Assert
			await action.Should().ThrowAsync<ArgumentNullException>();
		}

		[Fact]
		public async Task OrderItemAdderService_MissingOrder_ThrowsKeyNotFoundException()
		{
			//Arrange
			OrderItemAddRequest? request = _fixture.Create<OrderItemAddRequest>();

			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.GetOrderByOrderID(It.IsAny<Guid>())).ReturnsAsync(null as Order);

			//Act
			Func<Task> action = async () =>
			{
				await _orderItemAdderService.AddOrderItem(request);
			};

			//Assert
			await action.Should().ThrowAsync<KeyNotFoundException>();
		}

		[Fact]
		public async Task OrderItemAdderService_ValidRequest_ReturnsOrderItemResponse()
		{
			//Arrange
			Order order = _fixture.Create<Order>();
			OrderItemAddRequest? request = _fixture.Build<OrderItemAddRequest>()
				.With(r => r.OrderId, order.OrderId).Create();
			OrderItem item = request.ToOrderItem();

			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.GetOrderByOrderID(It.Is<Guid>(g => g.Equals(order.OrderId)))).ReturnsAsync(order);
			_unitOfWorkMock.Setup(temp => temp.OrderItemsRepository.AddOrderItem(It.IsAny<OrderItem>())).ReturnsAsync(item);

			OrderItemResponse expectedResponse = item.ToResponse();

			//Act
			OrderItemResponse actualResponseFromAdd = await _orderItemAdderService.AddOrderItem(request);

			//Assert
			expectedResponse.OrderItemId = actualResponseFromAdd.OrderItemId;

			actualResponseFromAdd.Should().BeEquivalentTo(expectedResponse);
		}

		#endregion

		#region OrderItemGetterService

		[Fact]
		public async Task OrderItemGetterService_GetAllOrderItems_OrderItemResponseList()
		{
			//Arrange
			List<OrderItem> items = _fixture.Create<List<OrderItem>>();

			_unitOfWorkMock.Setup(temp => temp.OrderItemsRepository.GetAllOrderItems()).ReturnsAsync(items);

			List<OrderItemResponse> expectedOrderItemResponses = items.Select(i => i.ToResponse()).ToList();

			//Act
			List<OrderItemResponse> actualOrderItemResponsesFromGet = await _orderItemGetterService.GetAllOrderItems();

			//Assert
			actualOrderItemResponsesFromGet.Should().BeEquivalentTo(expectedOrderItemResponses);
		}

		[Fact]
		public async Task OrderItemGetterService_GetOrderItemByOrderItemId_MissingItem_ThrowsKeyNotFoundException()
		{
			//Arrange
			Guid itemId = Guid.NewGuid();

			_unitOfWorkMock.Setup(temp => temp.OrderItemsRepository.GetOrderItemByOrderItemId(It.IsAny<Guid>())).ReturnsAsync(null as OrderItem);

			//Act
			Func<Task> action = async () =>
			{
				await _orderItemGetterService.GetOrderItemByOrderItemId(itemId);
			};

			//Assert
			await action.Should().ThrowAsync<KeyNotFoundException>();
		}

		[Fact]
		public async Task OrderItemGetterService_GetOrderItemByOrderItemId_MatchingItem_ReturnsItemResponse()
		{
			//Arrange
			OrderItem item = _fixture.Create<OrderItem>();
			Guid itemId = item.OrderItemId;

			_unitOfWorkMock.Setup(temp => temp.OrderItemsRepository.GetOrderItemByOrderItemId(It.Is<Guid>(g => g.Equals(itemId)))).ReturnsAsync(item);
			OrderItemResponse expectedOrderItemResponse = item.ToResponse();

			//Act
			OrderItemResponse actualOrderItemResponseFromGet = await _orderItemGetterService.GetOrderItemByOrderItemId(itemId);

			//Assert
			actualOrderItemResponseFromGet.Should().BeEquivalentTo(expectedOrderItemResponse);
		}

		[Fact]
		public async Task OrderItemGetterService_GetOrderItemsByOrderId_OrderItemResponses()
		{
			//Arrange
			Guid orderId = Guid.NewGuid();
			List<OrderItem> orderItemsFromSameOrder = _fixture.Build<OrderItem>()
				.With(oi => oi.OrderId, orderId)
				.CreateMany(5).ToList();


			List<OrderItemResponse> expectedOrderItemResponses = orderItemsFromSameOrder.Select(oi => oi.ToResponse()).ToList();

			_unitOfWorkMock.Setup(temp => temp.OrderItemsRepository.GetOrderItemsByOrderId(It.Is<Guid>(g => g.Equals(orderId)))).ReturnsAsync(orderItemsFromSameOrder);

			//Act
			List<OrderItemResponse> actualOrderItemResponsesFromGet = await _orderItemGetterService.GetOrderItemsByOrderId(orderId);

			//Assert
			actualOrderItemResponsesFromGet.Should().BeEquivalentTo(expectedOrderItemResponses);
		}

		#endregion

		#region OrderItemDeleterService

		[Fact]
		public async Task OrderItemDeleterService_DeleteOrderItemsByOrderId_MissingItem_ThrowsKeyNotFoundException()
		{
			//Arrange

			_unitOfWorkMock.Setup(temp => temp.OrderItemsRepository.GetOrderItemByOrderItemId(It.IsAny<Guid>())).ReturnsAsync(null as OrderItem);

			//Act
			Func<Task> action = async () =>
			{
				await _orderItemDeleterService.DeleteOrderItemByOrderItemId(Guid.NewGuid());
			};

			//Assert
			await action.Should().ThrowAsync<KeyNotFoundException>();
		}

		[Fact]
		public async Task OrderItemDeleterService_DeleteOrderItemByItemId_MatchingItem_ReturnsTrue()
		{
			//Arrange
			OrderItem item = _fixture.Create<OrderItem>();
			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.GetOrderByOrderID(It.IsAny<Guid>())).ReturnsAsync(null as Order);
			_unitOfWorkMock.Setup(temp => temp.OrderItemsRepository.GetOrderItemByOrderItemId(It.Is<Guid>(g => g.Equals(item.OrderItemId)))).ReturnsAsync(item);
			_unitOfWorkMock.Setup(temp => temp.OrderItemsRepository.DeleteOrderItemByItemId(It.Is<Guid>(g => g.Equals(item.OrderItemId)))).ReturnsAsync(true);
			_unitOfWorkMock.Setup(temp => temp.SaveAsync()).ReturnsAsync(1);

			//Act
			bool result = await _orderItemDeleterService.DeleteOrderItemByOrderItemId(item.OrderItemId);

			//Assert
			result.Should().BeTrue();
		}

		[Fact]
		public async Task OrderItemDeleterService_DeleteOrderItemsByOrderId_MatchingItem_ReturnsTrue()
		{
			//Arrange
			Guid orderId = Guid.NewGuid();
			_unitOfWorkMock.Setup(temp => temp.OrderItemsRepository.DeleteOrderItemsByOrderId(It.IsAny<Guid>())).ReturnsAsync(100);
			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.GetOrderByOrderID(It.IsAny<Guid>())).ReturnsAsync(null as Order);
			_unitOfWorkMock.Setup(temp => temp.SaveAsync()).ReturnsAsync(1);

			//Act
			bool result = await _orderItemDeleterService.DeleteOrderItemsByOrderId(orderId);

			//Assert

			result.Should().BeTrue();
		}

		#endregion

		#region OrderItemUpdaterService

		[Fact]
		public async Task OrderItemUpdaterService_UpdateOrderItem_NullRequest_ThrowsArgumentNullRequest()
		{
			//Arrange
			OrderItemUpdateRequest? request = null;

			//Act
			Func<Task> action = async () =>
			{
				await _orderItemUpdaterService.UpdateOrderItem(request);
			};

			//Assert
			await action.Should().ThrowAsync<ArgumentNullException>();
		}

		[Fact]
		public async Task OrderItemUpdaterService_UpdateOrderItem_InvalidRequest_ThrowsArgumentException()
		{
			//Arrange
			OrderItemUpdateRequest? request = _fixture.Build<OrderItemUpdateRequest>()
				.With(r => r.TotalPrice, -1)
				.Create();

			//Act
			Func<Task> action = async () =>
			{
				await _orderItemUpdaterService.UpdateOrderItem(request);
			};

			//Assert
			await action.Should().ThrowAsync<ArgumentException>();
		}

		[Fact]
		public async Task OrderItemUpdaterService_UpdateOrderItem_MissingOrder_ThrowsKeyNotFound()
		{
			//Arrange
			OrderItemUpdateRequest request = _fixture.Create<OrderItemUpdateRequest>();
			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.GetOrderByOrderID(It.IsAny<Guid>())).ReturnsAsync(null as Order);

			//Act
			Func<Task> action = async () =>
			{
				await _orderItemUpdaterService.UpdateOrderItem(request);
			};

			//Assert
			await action.Should().ThrowAsync<ApplicationException>();
		}


		[Fact]
		public async Task OrderItemUpdaterService_UpdateOrderItem_ValidRequest_ReturnsOrderItemResponse()
		{
			//Arrange
			Order order = _fixture.Create<Order>();
			OrderItemUpdateRequest request = _fixture.Create<OrderItemUpdateRequest>();
			OrderItemResponse expectedResponse = request.ToOrderItem().ToResponse();
			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.GetOrderByOrderID(It.IsAny<Guid>())).ReturnsAsync(order);
			_unitOfWorkMock.Setup(temp => temp.OrderItemsRepository.UpdateOrderItem(It.IsAny<OrderItem>())).ReturnsAsync(0);

			//Act
			OrderItemResponse actualResponseFromUpdate = await _orderItemUpdaterService.UpdateOrderItem(request);

			//Assert
			actualResponseFromUpdate.Should().BeEquivalentTo(expectedResponse);


		}

		#endregion

	}
}
