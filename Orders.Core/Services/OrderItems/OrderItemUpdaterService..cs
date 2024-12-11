using Microsoft.Extensions.Logging;
using Orders.Core.Domain.Entities;
using Orders.Core.Domain.RepositoryContracts;
using Orders.Core.DTO;
using Orders.Core.Helpers;
using Orders.Core.ServiceContracts.OrderItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.Services.OrderItems
{
	public class OrderItemUpdaterService : IOrderItemUpdaterService
	{
		private readonly ILogger<OrderItemUpdaterService> _logger;
		private readonly IUnitOfWork _unitOfWork;

		public OrderItemUpdaterService(ILogger<OrderItemUpdaterService> logger, IUnitOfWork unitOfWork)
		{
			_logger = logger;
			_unitOfWork = unitOfWork;
		}

		public async Task<OrderItemResponse> UpdateOrderItem(OrderItemUpdateRequest? orderItemUpdateRequest)
		{
			_logger.LogInformation($"{nameof(OrderItemUpdaterService)}/{nameof(UpdateOrderItem)} Updating order-item {orderItemUpdateRequest?.OrderItemId}");
			ArgumentNullException.ThrowIfNull(orderItemUpdateRequest, nameof(orderItemUpdateRequest));
			ValidationHelper.ValidateModel(orderItemUpdateRequest);

			try
			{
				Order? order = await _unitOfWork.OrdersRepository.GetOrderByOrderID(orderItemUpdateRequest.OrderId);

				if (order == null)
				{
					throw new KeyNotFoundException("Matching order not found");
				}
				decimal deltaPrice = await _unitOfWork.OrderItemsRepository.UpdateOrderItem(orderItemUpdateRequest.ToOrderItem());
				order.TotalAmount += deltaPrice;

				await _unitOfWork.SaveAsync();
				_logger.LogInformation($"{nameof(OrderItemUpdaterService)}/{nameof(UpdateOrderItem)}\n Update successful");
				return orderItemUpdateRequest.ToOrderItem().ToResponse();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while updating the order item");
				throw new ApplicationException();
			}

		}
	}
}
