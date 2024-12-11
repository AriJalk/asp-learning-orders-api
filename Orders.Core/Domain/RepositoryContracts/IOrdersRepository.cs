using Orders.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.Domain.RepositoryContracts
{
	public interface IOrdersRepository
	{
		Task<Order> AddOrder(Order order);

		Task<List<Order>> GetAllOrders();

		Task<Order?> GetOrderByOrderID(Guid orderId);

		Task<Order> UpdateOrder(Order order);

		Task<bool> DeleteOrder(Guid orderId);

		Task<List<Order>> GetFilteredOrders(Expression<Func<Order, bool>> predicate);

		Task<long> GetNextSequenceNumber();
	}
}
