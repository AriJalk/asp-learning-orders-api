using Microsoft.Extensions.Logging;
using Orders.Core.Domain.Entities;
using Orders.Core.Domain.RepositoryContracts;
using Orders.Core.ServiceContracts.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.Services.Orders
{
	public class OrderDeleterService : IOrderDeleterService
	{
		private readonly ILogger<OrderDeleterService> _logger;
		private readonly IUnitOfWork _unitOfWork;

		public OrderDeleterService(ILogger<OrderDeleterService> logger, IUnitOfWork unitOfWork)
		{
			_logger = logger;
			_unitOfWork = unitOfWork;
		}

		public async Task<bool> DeleteOrderByOrderId(Guid? orderId)
		{
			ArgumentNullException.ThrowIfNull(orderId);
			_logger.LogInformation($"{nameof(OrderDeleterService)} Deleting order {orderId}");
			try
			{
				Order? order = await _unitOfWork.OrdersRepository.GetOrderByOrderID(orderId.Value);
				if (order == null)
				{
					_logger.LogWarning("Order not found");
					throw new KeyNotFoundException("Key not found");
				}

				await _unitOfWork.OrderItemsRepository.DeleteOrderItemsByOrderId(orderId.Value);
				bool result = await _unitOfWork.OrdersRepository.DeleteOrder(orderId.Value);
				await _unitOfWork.SaveAsync();
				_logger.LogInformation($"Deletion status: {result}");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while deleting the order");
				throw;
			}
		}
	}
}
