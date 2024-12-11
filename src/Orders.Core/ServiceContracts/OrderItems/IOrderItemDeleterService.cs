using Orders.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.ServiceContracts.OrderItems
{
	/// <summary>
	/// Service for deletion of existing Order-Items
	/// </summary>
	public interface IOrderItemDeleterService
	{
		/// <summary>
		/// Delete Order-Item based on it's own Guid and update the matching order total.
		/// </summary>
		/// <param name="orderItemId"></param>
		/// <returns>True on successful deletion, false on failure</returns>
		Task<bool> DeleteOrderItemByOrderItemId(Guid orderItemId);

		/// <summary>
		/// Delete all related Order-Items based on OrderId
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns>True on successful deletion, false on failure</returns>
		Task<bool> DeleteOrderItemsByOrderId(Guid orderId);
	}
}
