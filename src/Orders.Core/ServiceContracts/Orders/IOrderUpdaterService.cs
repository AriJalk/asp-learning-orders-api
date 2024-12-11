using Orders.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.ServiceContracts.Orders
{
	/// <summary>
	/// Service for updating existing orders
	/// </summary>
	public interface IOrderUpdaterService
	{
		/// <summary>
		/// Update the order based on matching OrderId
		/// </summary>
		/// <param name="updateRequest">Update DTO with the new information</param>
		/// <returns>OrderResponse of the updated order details</returns>
		Task<OrderResponse> UpdateOrder(OrderUpdateRequest? updateRequest);
	}
}
