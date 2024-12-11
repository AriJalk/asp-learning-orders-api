using Orders.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.ServiceContracts.OrderItems
{
	/// <summary>
	/// Service for retrieval of Order-Items
	/// </summary>
	public interface IOrderItemGetterService
	{
		/// <summary>
		/// Gets matching OrderItem based on OrderItemId
		/// </summary>
		/// <param name="orderItemId">Guid of the order-item</param>
		/// <returns>OrderItemResponse</returns>
		Task<OrderItemResponse> GetOrderItemByOrderItemId(Guid orderItemId);

		/// <summary>
		/// Gets Order-Items matching the OrderId
		/// </summary>
		/// <param name="orderId">The required OrderId</param>
		/// <returns>Matching OrderItems corresponding to the OrderId</returns>
		Task<List<OrderItemResponse>> GetOrderItemsByOrderId(Guid orderId);

		/// <summary>
		/// Gets all OrderItems in the database
		/// </summary>
		/// <returns>List of OrderItemResponses</returns>
		Task<List<OrderItemResponse>> GetAllOrderItems();
	}
}
