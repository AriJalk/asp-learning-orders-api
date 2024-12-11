using Microsoft.Extensions.Logging;
using Orders.Core.Domain.Entities;
using Orders.Core.Domain.RepositoryContracts;
using Orders.Core.DTO;
using Orders.Core.Helpers;
using Orders.Core.ServiceContracts.OrderItems;
using Orders.Core.ServiceContracts.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.Services.Orders
{
	public class OrderGetterService : IOrderGetterService
	{
		private readonly ILogger<OrderGetterService> _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IOrderItemGetterService _itemGetterService;

		public OrderGetterService(ILogger<OrderGetterService> logger, IUnitOfWork unitOfWork, IOrderItemGetterService orderItemGetterService)
		{
			_logger = logger;
			_unitOfWork = unitOfWork;
			_itemGetterService = orderItemGetterService;
		}

		public async Task<List<OrderResponse>> GetAllOrders()
		{
			_logger.LogInformation($"{nameof(OrderGetterService)}/{nameof(GetAllOrders)}");
			List<OrderResponse> orderResponses = (await _unitOfWork.OrdersRepository.GetAllOrders()).Select(o => o.ToOrderResponse()).ToList();
			foreach (OrderResponse response in orderResponses)
			{
				response.OrderItems = await _itemGetterService.GetOrderItemsByOrderId(response.OrderId);
			}

			return orderResponses;

		}

		public async Task<OrderResponse> GetOrderByOrderId(Guid? orderId)
		{
			_logger.LogInformation($"{nameof(OrderGetterService)}/{nameof(GetOrderByOrderId)}");
			ArgumentNullException.ThrowIfNull(orderId);
			
			Order? order = await _unitOfWork.OrdersRepository.GetOrderByOrderID(orderId.Value);
			if (order == null)
			{
				throw new KeyNotFoundException("Order Key not found");
			}

			OrderResponse response = order.ToOrderResponse();
			response.OrderItems = await _itemGetterService.GetOrderItemsByOrderId(order.OrderId);

			return response;
		}
	}
}
