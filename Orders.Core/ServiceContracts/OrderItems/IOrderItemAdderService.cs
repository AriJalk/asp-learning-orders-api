using Orders.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.ServiceContracts.OrderItems
{
	/// <summary>
	/// Service for adding new Order-Items to an existing order
	/// </summary>
	public interface IOrderItemAdderService
	{
		/// <summary>
		/// Add an Order-Item to an existing order
		/// </summary>
		/// <param name="orderItemAddRequest"></param>
		/// <returns>Order-Item response on successful adding</returns>
		Task<OrderItemResponse> AddOrderItem(OrderItemAddRequest? orderItemAddRequest);
	}
}
