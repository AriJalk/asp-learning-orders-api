using MediatR;
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
	public class OrderItemAdderService : IOrderItemAdderService
	{
		private readonly ILogger<OrderItemAdderService> _logger;
		private readonly IUnitOfWork _unitOfWork;

		public OrderItemAdderService(ILogger<OrderItemAdderService> logger, IUnitOfWork unitOfWork)
		{
			_logger = logger;
			_unitOfWork = unitOfWork;
		}

		public async Task<OrderItemResponse> AddOrderItem(OrderItemAddRequest? orderItemAddRequest)
		{
			_logger.LogInformation($"{nameof(OrderItemAdderService)}/{nameof(AddOrderItem)}\nAdding order: {orderItemAddRequest?.ToString()}");
			ArgumentNullException.ThrowIfNull(orderItemAddRequest, nameof(OrderItemAddRequest));
			try
			{

				Order? order = await _unitOfWork.OrdersRepository.GetOrderByOrderID(orderItemAddRequest.OrderId);
				if (order == null)
				{
					//TODO: other exception
					throw new KeyNotFoundException();
				}


				OrderItem item = orderItemAddRequest.ToOrderItem();
				item.OrderItemId = Guid.NewGuid();


				await _unitOfWork.OrderItemsRepository.AddOrderItem(item);
				order.TotalAmount += item.TotalPrice;

				await _unitOfWork.SaveAsync();
				_logger.LogInformation($"{nameof(OrderItemAdderService)}/{nameof(AddOrderItem)}\nOrder-Item added successfuly");
				return item.ToResponse();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while adding the order item");
				throw;
			}
		}
	}
}
