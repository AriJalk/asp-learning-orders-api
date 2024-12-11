using Orders.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.Domain.RepositoryContracts
{
	public interface IOrderItemsRepository
	{
		Task<OrderItem> AddOrderItem(OrderItem item);

		Task<decimal> UpdateOrderItem(OrderItem item);
		Task<bool> DeleteOrderItemByItemId(Guid itemId);
		Task<decimal> DeleteOrderItemsByOrderId(Guid orderId);
		Task<List<OrderItem>> GetAllOrderItems();

		Task<OrderItem?> GetOrderItemByOrderItemId(Guid orderItemId);
		Task<List<OrderItem>> GetOrderItemsByOrderId(Guid orderId);

	}
}
