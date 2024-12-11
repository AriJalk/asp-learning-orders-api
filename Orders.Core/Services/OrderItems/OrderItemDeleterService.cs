using Microsoft.Extensions.Logging;
using Orders.Core.Domain.Entities;
using Orders.Core.Domain.RepositoryContracts;
using Orders.Core.DTO;
using Orders.Core.ServiceContracts.OrderItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.Services.OrderItems
{
	public class OrderItemDeleterService : IOrderItemDeleterService
	{
		private readonly ILogger<OrderItemDeleterService> _logger;
		private readonly IUnitOfWork _unitOfWork;

		public OrderItemDeleterService(ILogger<OrderItemDeleterService> logger, IUnitOfWork unitOfWork)
		{
			_logger = logger;
			_unitOfWork = unitOfWork;
		}

		public async Task<bool> DeleteOrderItemByOrderItemId(Guid orderItemId)
		{
			_logger.LogInformation($"{nameof(OrderItemDeleterService)}/{nameof(DeleteOrderItemByOrderItemId)}\nDeleting order-item: {orderItemId}");
			try
			{
				OrderItem? item = await _unitOfWork.OrderItemsRepository.GetOrderItemByOrderItemId(orderItemId);
				if (item == null)
				{
					throw new KeyNotFoundException("Order-Item not found");
				}
				Order? order = await _unitOfWork.OrdersRepository.GetOrderByOrderID(item.OrderId);
				if (order != null)
				{
					order.TotalAmount -= item.TotalPrice;
				}
				await _unitOfWork.OrderItemsRepository.DeleteOrderItemByItemId(orderItemId);
				int deletedRows = await _unitOfWork.SaveAsync();
				_logger.LogInformation($"{nameof(OrderItemDeleterService)}/{nameof(DeleteOrderItemByOrderItemId)}\nDelete status: {deletedRows > 0}");
				return deletedRows > 0;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while deleting the order item");
				throw;
			}
		}

		public async Task<bool> DeleteOrderItemsByOrderId(Guid orderId)
		{
			try
			{
				decimal totalPrice = await _unitOfWork.OrderItemsRepository.DeleteOrderItemsByOrderId(orderId);
				Order? order = await _unitOfWork.OrdersRepository.GetOrderByOrderID(orderId);
				if (order != null)
				{
					order.TotalAmount -= totalPrice;
				}

				return await _unitOfWork.SaveAsync() > 0;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while deleting the order items");
				throw;
			}
		}
	}
}
