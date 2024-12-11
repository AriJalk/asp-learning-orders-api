using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.ServiceContracts.Orders
{
	/// <summary>
	/// Order-Deletion service
	/// </summary>
	public interface IOrderDeleterService
	{
		/// <summary>
		/// Deletes order with matching orderId
		/// </summary>
		/// <param name="orderId">Guid of the order to be deleted</param>
		/// <returns>True if deletion occurred</returns>
		Task<bool> DeleteOrderByOrderId(Guid? orderId);
	}
}
