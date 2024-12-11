using Microsoft.Extensions.Logging;
using Orders.Core.Domain.Entities;
using Orders.Core.Domain.RepositoryContracts;
using Orders.Core.DTO;
using Orders.Core.Helpers;
using Orders.Core.ServiceContracts.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.Services.Orders
{
	public class OrderUpdaterService : IOrderUpdaterService
	{
		private readonly ILogger<OrderUpdaterService> _logger;
		private readonly IUnitOfWork _unitOfWork;

		public OrderUpdaterService(ILogger<OrderUpdaterService> logger, IUnitOfWork unitOfWork)
		{
			_logger = logger;
			_unitOfWork = unitOfWork;
		}

		public async Task<OrderResponse> UpdateOrder(OrderUpdateRequest? updateRequest)
		{
			_logger.LogInformation($"{nameof(OrderUpdaterService)}/{nameof(UpdateOrder)}\nUpdating order: {updateRequest?.OrderId}");
			ArgumentNullException.ThrowIfNull(updateRequest, nameof(updateRequest));

			ValidationHelper.ValidateModel(updateRequest);

			try
			{
				Order? order = await _unitOfWork.OrdersRepository.GetOrderByOrderID(updateRequest.OrderId);
				if (order == null)
				{
					throw new KeyNotFoundException("Key not found");
				}

				order.CustomerName = updateRequest.CustomerName;
				order.TotalAmount = updateRequest.TotalAmount;

				Order updatedOrder = await _unitOfWork.OrdersRepository.UpdateOrder(order);
				await _unitOfWork.SaveAsync();
				return updatedOrder.ToOrderResponse();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while updating the order");
				throw;
			}
		}
	}
}
