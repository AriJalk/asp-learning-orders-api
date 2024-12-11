using Orders.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.ServiceContracts.OrderItems
{
	/// <summary>
	/// Service for updating existing Order-Items
	/// </summary>
	public interface IOrderItemUpdaterService
	{
		/// <summary>
		/// Update the matching order
		/// </summary>
		/// <param name="orderItemUpdateRequest">Update details</param>
		/// <returns>Updated OrderItem details</returns>
		Task<OrderItemResponse> UpdateOrderItem(OrderItemUpdateRequest? orderItemUpdateRequest);
	}
}
