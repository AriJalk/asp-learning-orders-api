using Orders.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.ServiceContracts.Orders
{
	/// <summary>
	/// Service for filtering orders
	/// </summary>
	public interface IOrderFilterService
	{
		/// <summary>
		/// Gets all matching orders
		/// </summary>
		/// <param name="searchBy">Order-Property to searchBy</param>
		/// <param name="searchString">The value to be searched</param>
		/// <returns>List of OrderResponses with matching search orders</returns>
		Task<List<OrderResponse>> GetFilteredOrders(string searchBy, string? searchString);

	}
}
