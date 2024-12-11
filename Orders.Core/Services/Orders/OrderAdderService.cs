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
	public class OrderAdderService : IOrderAdderService
	{
		private readonly ILogger<OrderAdderService> _logger;
		private readonly IUnitOfWork _unitOfWork;

		public OrderAdderService(ILogger<OrderAdderService> logger, IUnitOfWork unitOfWork)
		{
			_logger = logger;
			_unitOfWork = unitOfWork;
		}

		public async Task<OrderResponse> AddOrder(OrderAddRequest? orderAddRequest)
		{
			ArgumentNullException.ThrowIfNull(orderAddRequest);
			ArgumentException.ThrowIfNullOrEmpty(orderAddRequest.CustomerName);


			_logger.LogInformation($"{nameof(OrderAdderService)}\nRequest: {orderAddRequest.ToString()}\nAdding order");
			try
			{
				Order order = new Order();
				order.CustomerName = orderAddRequest.CustomerName;
				order.OrderId = Guid.NewGuid();
				order.OrderDate = DateTime.Today;
				long nextNumber = await _unitOfWork.OrdersRepository.GetNextSequenceNumber();
				order.OrderNumber = $"Order_{DateTime.Today.Year}_{nextNumber:D5}";

				decimal total = 0;
				List<OrderItemResponse> itemResponses = new List<OrderItemResponse>();
				foreach (OrderItemAddRequest itemAddRequest in orderAddRequest.OrderItems)
				{
					_logger.LogInformation($"{nameof(OrderAdderService)}\nAdding order-item: {itemAddRequest.ToString()}");
					itemAddRequest.OrderId = order.OrderId;
					total += itemAddRequest.TotalPrice;
					OrderItem orderItem = itemAddRequest.ToOrderItem();
					orderItem.OrderItemId = Guid.NewGuid();
					await _unitOfWork.OrderItemsRepository.AddOrderItem(orderItem);
					itemResponses.Add(orderItem.ToResponse());
				}

				order.TotalAmount = total;

				await _unitOfWork.OrdersRepository.AddOrder(order);

				OrderResponse orderResponse = order.ToOrderResponse();
				orderResponse.OrderItems = itemResponses;
				await _unitOfWork.SaveAsync();
				_logger.LogInformation($"{nameof(OrderAdderService)} => Order added successfuly");
				return orderResponse;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "An error occurred while adding the order");
				throw;
			}
		}
	}
}
