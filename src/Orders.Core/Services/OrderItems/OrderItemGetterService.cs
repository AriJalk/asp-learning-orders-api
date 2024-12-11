using Microsoft.Extensions.Logging;
using Orders.Core.Domain.Entities;
using Orders.Core.Domain.RepositoryContracts;
using Orders.Core.DTO;
using Orders.Core.ServiceContracts.OrderItems;
using Orders.Core.ServiceContracts.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.Services.OrderItems
{
	public class OrderItemGetterService : IOrderItemGetterService
	{
		private readonly ILogger<OrderItemGetterService> _logger;
		private readonly IUnitOfWork _unitOfWork;

		public OrderItemGetterService(ILogger<OrderItemGetterService> logger, IUnitOfWork unitOfWork)
		{
			_logger = logger;
			_unitOfWork = unitOfWork;
		}

		public async Task<List<OrderItemResponse>> GetAllOrderItems()
		{
			_logger.LogInformation($"{nameof(OrderItemGetterService)}/{GetAllOrderItems}\nGetting all order-items");
			return (await _unitOfWork.OrderItemsRepository.GetAllOrderItems()).Select(oi => oi.ToResponse()).ToList();
		}

		public async Task<OrderItemResponse> GetOrderItemByOrderItemId(Guid orderItemId)
		{
			_logger.LogInformation($"{nameof(OrderItemGetterService)}/{GetOrderItemByOrderItemId}\nGetting order-item {orderItemId}");
			OrderItem? item = await _unitOfWork.OrderItemsRepository.GetOrderItemByOrderItemId(orderItemId);
			if (item == null)
				throw new KeyNotFoundException("Order-Item not found");
			return item.ToResponse();
		}

		public async Task<List<OrderItemResponse>> GetOrderItemsByOrderId(Guid orderId)
		{
			_logger.LogInformation($"{nameof(OrderItemGetterService)}/{GetOrderItemsByOrderId}\nGetting all order-items by OrderId: {orderId}");
			return (await _unitOfWork.OrderItemsRepository.GetOrderItemsByOrderId(orderId)).Select(oi => oi.ToResponse()).ToList();
		}
	}
}
