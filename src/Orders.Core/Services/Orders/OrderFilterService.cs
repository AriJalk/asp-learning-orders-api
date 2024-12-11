using Microsoft.Extensions.Logging;
using Orders.Core.Domain.Entities;
using Orders.Core.Domain.RepositoryContracts;
using Orders.Core.DTO;
using Orders.Core.ServiceContracts.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.Services.Orders
{
	public class OrderFilterService : IOrderFilterService
	{
		private readonly ILogger<OrderFilterService> _logger;
		private readonly IUnitOfWork _unitOfWork;

		public OrderFilterService(ILogger<OrderFilterService> logger, IUnitOfWork unitOfWork)
		{
			_logger = logger;
			_unitOfWork = unitOfWork;
		}

		public async Task<List<OrderResponse>> GetFilteredOrders(string searchBy, string? searchString)
		{
			_logger.LogInformation($"{nameof(OrderFilterService)}/{nameof(GetFilteredOrders)}");
			List<Order> filteredOrders;
			filteredOrders = searchBy switch
			{
				nameof(Order.OrderId) => await _unitOfWork.OrdersRepository.GetFilteredOrders(order => order.OrderId.ToString().Contains(searchString)),

				nameof(Order.OrderDate) => await _unitOfWork.OrdersRepository.GetFilteredOrders(order => order.OrderDate.Value.ToString("dd MM yyyy").Contains(searchString)),

				nameof(Order.OrderNumber) => await _unitOfWork.OrdersRepository.GetFilteredOrders(order => order.OrderNumber.Contains(searchString)),

				nameof(Order.CustomerName) => await _unitOfWork.OrdersRepository.GetFilteredOrders(order => order.CustomerName.Contains(searchString)),

				nameof(Order.TotalAmount) => await _unitOfWork.OrdersRepository.GetFilteredOrders(order => order.TotalAmount.ToString().Contains(searchString)),

				_ =>
						await _unitOfWork.OrdersRepository.GetAllOrders(),
			};

			return filteredOrders.Select(o => o.ToOrderResponse()).ToList();
		}
	}
}
