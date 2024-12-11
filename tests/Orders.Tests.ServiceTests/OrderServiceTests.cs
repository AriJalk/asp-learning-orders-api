using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.Core.Domain.Entities;
using Orders.Core.Domain.RepositoryContracts;
using Orders.Core.DTO;
using Orders.Core.ServiceContracts.OrderItems;
using Orders.Core.ServiceContracts.Orders;
using Orders.Core.Services.Orders;
using Orders.Infrastructure.Repositories;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Orders.Tests.ServiceTests
{
	public class OrderServiceTests
	{
		private readonly IOrderAdderService _orderAdderService;
		private readonly IOrderGetterService _orderGetterService;
		private readonly IOrderFilterService _orderFilterService;
		private readonly IOrderUpdaterService _orderUpdaterService;
		private readonly IOrderDeleterService _orderDeleterService;

		private readonly Mock<IUnitOfWork> _unitOfWorkMock;
		private readonly IUnitOfWork _unitOfWork;

		private readonly Mock<IOrderItemGetterService> _itemGetterServiceMock;
		private readonly IOrderItemGetterService _itemGetterService;

		private readonly IFixture _fixture;

		public OrderServiceTests()
		{
			_fixture = new Fixture();
			_unitOfWorkMock = new Mock<IUnitOfWork>();
			_unitOfWork = _unitOfWorkMock.Object;

			_itemGetterServiceMock = new Mock<IOrderItemGetterService>();
			_itemGetterService = _itemGetterServiceMock.Object;

			_orderAdderService = new OrderAdderService(new Mock<ILogger<OrderAdderService>>().Object, _unitOfWork);
			_orderGetterService = new OrderGetterService(new Mock<ILogger<OrderGetterService>>().Object, _unitOfWork, _itemGetterService);
			_orderFilterService = new OrderFilterService(new Mock<ILogger<OrderFilterService>>().Object, _unitOfWork);
			_orderUpdaterService = new OrderUpdaterService(new Mock<ILogger<OrderUpdaterService>>().Object, _unitOfWork);
			_orderDeleterService = new OrderDeleterService(new Mock<ILogger<OrderDeleterService>>().Object, _unitOfWork);

		}

		#region OrderAdderService

		[Fact]
		public async Task OrderAdderService_AddOrder_NullRequest_ThrowsArgumentNullException()
		{
			//Arrange
			OrderAddRequest? request = null;
			//Act
			Func<Task> action = async () =>
			{
				await _orderAdderService.AddOrder(request);
			};
			//Assert
			await action.Should().ThrowAsync<ArgumentNullException>();
		}

		[Fact]
		public async Task OrderAdderService_AddOrder_NullCustomerName_ThrowsArgumentException()
		{
			//Arrange
			OrderAddRequest? request = _fixture.Build<OrderAddRequest>().With(oar => oar.CustomerName, null as string).Create();
			//Act
			Func<Task> action = async () =>
			{
				await _orderAdderService.AddOrder(request);
			};
			//Assert
			await action.Should().ThrowAsync<ArgumentException>();
		}

		[Fact]
		public async Task OrderAdderService_AddOrder_ValidOrder_SuccessfulOrderResponse()
		{
			//Arrange
			List<OrderItemAddRequest> itemsToAdd = _fixture.CreateMany<OrderItemAddRequest>().ToList();
			itemsToAdd.ForEach(item => item.ProductName = Guid.NewGuid().ToString());
			OrderAddRequest addRequest = _fixture.Create<OrderAddRequest>();
			Order order = _fixture.Build<Order>()
				.With(o => o.CustomerName, addRequest.CustomerName)
				.With(o => o.TotalAmount, addRequest.OrderItems.Sum(oi => oi.TotalPrice))
				.Create();
			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.GetNextSequenceNumber()).ReturnsAsync(1);
			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.AddOrder(It.IsAny<Order>())).ReturnsAsync(new Order());
			_unitOfWorkMock.Setup(temp => temp.OrderItemsRepository.AddOrderItem(It.IsAny<OrderItem>())).ReturnsAsync(new OrderItem());

			//Act
			OrderResponse actualOrderResponseFromAdd = await _orderAdderService.AddOrder(addRequest);

			//Assert

			//Assign data from creation to order
			order.OrderNumber = actualOrderResponseFromAdd.OrderNumber;
			order.OrderId = actualOrderResponseFromAdd.OrderId;
			order.OrderDate = actualOrderResponseFromAdd.OrderDate;


			OrderResponse expectedOrderResponse = order.ToOrderResponse();
			foreach (OrderItemAddRequest orderItemAddRequest in addRequest.OrderItems)
			{
				OrderItemResponse response = orderItemAddRequest.ToOrderItem().ToResponse();
				response.OrderId = order.OrderId;
				OrderItemResponse? matchingResponse = actualOrderResponseFromAdd.OrderItems.Where(oi => oi.ProductName == response.ProductName).FirstOrDefault();
				if (matchingResponse != null)
				{
					response.OrderItemId = matchingResponse.OrderItemId;
				}
				expectedOrderResponse.OrderItems.Add(response);
			}

			actualOrderResponseFromAdd.Should().BeEquivalentTo(expectedOrderResponse);
		}

		#endregion

		#region OrderGetterService
		[Fact]
		public async Task GetOrderByOrderId_EmptyOrderId_ThrowsArgumentNullException()
		{
			//Arrange
			Guid? orderId = null;

			//Act
			Func<Task> action = async () =>
			{
				await _orderGetterService.GetOrderByOrderId(orderId);
			};

			//Assert
			await action.Should().ThrowAsync<ArgumentNullException>();
		}

		[Fact]
		public async Task GetOrderByOrderId_OrderIdNotFound_ThrowsKeyNotFound()
		{
			//Arrange
			Guid orderId = Guid.NewGuid();

			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.GetOrderByOrderID(It.IsAny<Guid>())).ReturnsAsync(null as Order);

			//Act
			Func<Task> action = async () =>
			{
				await _orderGetterService.GetOrderByOrderId(orderId);
			};

			//Assert
			await action.Should().ThrowAsync<KeyNotFoundException>();
		}

		[Fact]
		public async Task GetOrderByOrderId_OrderIdMatch_ReturnsExpectedOrderResponse()
		{
			//Arrange
			Order order = _fixture.Create<Order>();
			Guid orderId = order.OrderId;
			OrderResponse expectedOrderResponse = order.ToOrderResponse();
			List<OrderItemResponse> itemResponses = _fixture.Build<OrderItemResponse>().With(item => item.OrderId, orderId).CreateMany(5).ToList();
			expectedOrderResponse.OrderItems = itemResponses;
			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.GetOrderByOrderID(It.IsAny<Guid>())).ReturnsAsync(order);
			_itemGetterServiceMock.Setup(temp => temp.GetOrderItemsByOrderId(It.Is<Guid>(g => g.Equals(orderId)))).ReturnsAsync(itemResponses);

			//Act
			OrderResponse? actualOrderResponseFromGet = await _orderGetterService.GetOrderByOrderId(orderId);

			//Assert
			actualOrderResponseFromGet.Should().BeEquivalentTo(expectedOrderResponse);
		}

		[Fact]
		public async Task GetAllOrders_Empty_ShouldReturnEmptyList()
		{
			//Arrange
			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.GetAllOrders()).ReturnsAsync(new List<Order>());

			//Act
			List<OrderResponse> orderResponses = await _orderGetterService.GetAllOrders();

			//Assert
			orderResponses.Should().HaveCount(0);
		}

		[Fact]
		public async Task GetAllOrders_WithMultipleOrders_ReturnsCompleteOrderResponses()
		{
			//Arrange
			List<Order> orders = _fixture.CreateMany<Order>(3).ToList(); // Explicit count for clarity
			List<OrderResponse> expectedOrderResponses = orders.Select(o => o.ToOrderResponse()).ToList();

			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.GetAllOrders()).ReturnsAsync(orders);

			foreach (OrderResponse orderResponse in expectedOrderResponses)
			{
				List<OrderItemResponse> itemResponses = _fixture
					.Build<OrderItemResponse>()
					.With(oi => oi.OrderId, orderResponse.OrderId)
					.CreateMany(5)
					.ToList();

				_itemGetterServiceMock.Setup(service => service.GetOrderItemsByOrderId(It.Is<Guid>(g => g.Equals(orderResponse.OrderId))))
					.ReturnsAsync(itemResponses);

				orderResponse.OrderItems = itemResponses;
			}

			//Act
			List<OrderResponse> actualOrderResponsesFromGet = await _orderGetterService.GetAllOrders();

			//Assert
			actualOrderResponsesFromGet.Should().BeEquivalentTo(expectedOrderResponses);

			//Verify calls
			_unitOfWorkMock.Verify(temp => temp.OrdersRepository.GetAllOrders(), Times.Once);
			foreach (OrderResponse orderResponse in expectedOrderResponses)
			{
				_itemGetterServiceMock.Verify(service => service.GetOrderItemsByOrderId(It.Is<Guid>(g => g.Equals(orderResponse.OrderId))), Times.Once);
			}
		}

		#endregion

		#region OrderFilterService

		[Fact]
		public async Task GetFilteredOrders_EmptySearchText_ReturnsAllOrders()
		{
			//Arrange
			List<Order> orders = _fixture.CreateMany<Order>().ToList();
			List<OrderResponse> expectedOrderResponses = orders.Select(o => o.ToOrderResponse()).ToList();
			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.GetFilteredOrders(It.IsAny<Expression<Func<Order, bool>>>())).ReturnsAsync((Expression<Func<Order, bool>> filter) => orders.Where(filter.Compile()).ToList());

			//Act 
			List<OrderResponse> actualOrderResponsesFromFilter = await _orderFilterService.GetFilteredOrders(nameof(Order.OrderNumber), "");

			//Assert
			actualOrderResponsesFromFilter.Should().BeEquivalentTo(expectedOrderResponses);
		}

		[Fact]
		public async Task GetFilteredOrders_CustomerSearchTest_ReturnsMatchingOrders()
		{
			//Arrange
			string searchString = "11";
			List<Order> orders = new List<Order>()
			{
				_fixture.Build<Order>().With(o => o.CustomerName, "Test11").Create(),
				_fixture.Build<Order>().With(o => o.CustomerName, "Test112").Create(),
				_fixture.Build<Order>().With(o => o.CustomerName, "Test12").Create(),
				_fixture.Build<Order>().With(o => o.CustomerName, "Test121").Create(),
				_fixture.Build<Order>().With(o => o.CustomerName, "Test300").Create(),
				_fixture.Build<Order>().With(o => o.CustomerName, "Test0011").Create(),
			};

			List<OrderResponse> expectedOrderResponses = orders
				.Where(o => o.CustomerName.Contains(searchString))
				.Select(o => o.ToOrderResponse())
				.ToList();

			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.GetFilteredOrders(It.IsAny<Expression<Func<Order, bool>>>())).ReturnsAsync((Expression<Func<Order, bool>> filter) => orders.Where(filter.Compile()).ToList());



			//Act
			List<OrderResponse> actualOrderResponsesFromFilter = await _orderFilterService.GetFilteredOrders(nameof(Order.CustomerName), searchString);

			//Assert
			actualOrderResponsesFromFilter.Should().BeEquivalentTo(expectedOrderResponses);
		}

		#endregion

		#region OrderDeleterService
		[Fact]
		public async Task DeleteOrderByOrderId_NullOrderId_ThrowsArgumentNullException()
		{
			//Arrange
			Guid? id = null;

			//Act
			Func<Task> action = async () =>
			{
				await _orderDeleterService.DeleteOrderByOrderId(id);
			};

			//Assert
			await action.Should().ThrowAsync<ArgumentNullException>();
		}

		[Fact]
		public async Task DeleteOrderByOrderId_InvalidId_ThrowsKeyNotFound()
		{
			//Arrange
			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.GetOrderByOrderID(It.IsAny<Guid>())).ReturnsAsync(null as Order);
			Guid orderId = Guid.NewGuid();

			//Act
			Func<Task> action = async () =>
			{
				await _orderDeleterService.DeleteOrderByOrderId(orderId);
			};

			//Assert
			await action.Should().ThrowAsync<KeyNotFoundException>();
		}

		[Fact]
		public async Task DeleteOrderByOrderId_ValidOrderId_ReturnsTrue()
		{
			//Arrange
			Order order = _fixture.Create<Order>();

			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.GetOrderByOrderID(It.Is<Guid>(g => g.Equals(order.OrderId)))).ReturnsAsync(order);
			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.DeleteOrder(It.Is<Guid>(g => g.Equals(order.OrderId)))).ReturnsAsync(true);

			_unitOfWorkMock.Setup(temp => temp.OrderItemsRepository.DeleteOrderItemsByOrderId(It.IsAny<Guid>())).ReturnsAsync(0);

			//Act
			bool result = await _orderDeleterService.DeleteOrderByOrderId(order.OrderId);

			//Assert
			result.Should().BeTrue();
		}

		#endregion

		#region OrderUpdaterService
		[Fact]
		public async Task OrderUpdaterService_NullUpdateDTO_ThrowsArgumentNullException()
		{
			//Arrange
			OrderUpdateRequest? request = null;

			//Act
			Func<Task> action = async () =>
			{
				await _orderUpdaterService.UpdateOrder(request);
			};

			//Assert
			await action.Should().ThrowAsync<ArgumentNullException>();
		}

		[Fact]
		public async Task OrderUpdaterService_InvalidTotalAmount_ThrowsArgumentException()
		{
			//Arrange
			OrderUpdateRequest request = _fixture.Build<OrderUpdateRequest>()
				.With(r => r.TotalAmount, -1).Create();

			//Act
			Func<Task> action = async () =>
			{
				await _orderUpdaterService.UpdateOrder(request);
			};

			//Assert
			await action.Should().ThrowAsync<ArgumentException>();
		}

		[Fact]
		public async Task OrderUpdaterService_NullCustomerName_ThrowsArgumentException()
		{
			//Arrange
			OrderUpdateRequest request = _fixture.Build<OrderUpdateRequest>()
				.With(r => r.CustomerName, null as string).Create();

			//Act
			Func<Task> action = async () =>
			{
				await _orderUpdaterService.UpdateOrder(request);
			};

			//Assert
			await action.Should().ThrowAsync<ArgumentException>();
		}

		[Fact]
		public async Task OrderUpdaterService_InvalidOrderId_ThrowsArgumentException()
		{
			//Arrange
			OrderUpdateRequest request = _fixture.Create<OrderUpdateRequest>();

			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.GetOrderByOrderID(It.IsAny<Guid>())).ReturnsAsync(null as Order);

			//Act
			Func<Task> action = async () =>
			{
				await _orderUpdaterService.UpdateOrder(request);
			};

			//Assert
			await action.Should().ThrowAsync<KeyNotFoundException>();
		}

		[Fact]
		public async Task OrderUpdaterService_ValidRequest_ReturnsOrderResponse()
		{
			//Arrange
			Order originalOrder = _fixture.Create<Order>();
			OrderUpdateRequest updateRequest = new OrderUpdateRequest()
			{
				OrderId = originalOrder.OrderId,
				OrderDate = originalOrder.OrderDate.Value,
				OrderNumber = originalOrder.OrderNumber,
				TotalAmount = originalOrder.TotalAmount - 10,
				CustomerName = "NewName",
			};

			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.GetOrderByOrderID(It.Is<Guid>(g => g.Equals(originalOrder.OrderId))))
				.ReturnsAsync(originalOrder);
			Order updatedOrder = new Order()
			{
				OrderId = originalOrder.OrderId,
				OrderNumber = originalOrder.OrderNumber,
				OrderDate = originalOrder.OrderDate,
				CustomerName = updateRequest.CustomerName,
				TotalAmount = updateRequest.TotalAmount,
			};

			_unitOfWorkMock.Setup(temp => temp.OrdersRepository.UpdateOrder(It.IsAny<Order>()))
				.ReturnsAsync(updatedOrder);

			//Act
			OrderResponse actualResponseFromUpdate = await _orderUpdaterService.UpdateOrder(updateRequest);


			//Assert
			OrderResponse expectedResponseFromUpdate = updatedOrder.ToOrderResponse();
			actualResponseFromUpdate.Should().BeEquivalentTo(expectedResponseFromUpdate);
		}



		#endregion

	}
}