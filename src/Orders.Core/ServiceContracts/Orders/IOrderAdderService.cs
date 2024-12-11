using Orders.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.ServiceContracts.Orders
{
	/// <summary>
	/// Service for adding new orders
	/// </summary>
	public interface IOrderAdderService
	{
		/// <summary>
		/// Add a new order
		/// </summary>
		/// <param name="orderAddRequest">Order details with optional list of order items</param>
		/// <returns>OrderResponse with created order details</returns>
		Task<OrderResponse> AddOrder(OrderAddRequest? orderAddRequest);
	}
}
