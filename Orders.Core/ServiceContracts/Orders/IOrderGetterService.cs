using Orders.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.ServiceContracts.Orders
{
	/// <summary>
	/// Service for getting orders
	/// </summary>
	public interface IOrderGetterService
	{
		/// <summary>
		/// Get order by matching orderId
		/// </summary>
		/// <param name="orderId">The Guid of the requested order</param>
		/// <returns>OrderResponse with the order details</returns>
		Task<OrderResponse> GetOrderByOrderId(Guid? orderId);

		/// <summary>
		/// Get all orders in the database
		/// </summary>
		/// <returns>List of OrderResponses with order details</returns>
		Task<List<OrderResponse>> GetAllOrders();
	}
}
