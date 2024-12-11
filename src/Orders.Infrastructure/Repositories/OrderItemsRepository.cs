using Microsoft.EntityFrameworkCore;
using Orders.Core.Domain.Entities;
using Orders.Core.Domain.RepositoryContracts;
using Orders.Infrastructure.DatabaseContexts;

namespace Orders.Infrastructure.Repositories
{
	public class OrderItemsRepository : IOrderItemsRepository
	{
		private readonly ApplicationDbContext _db;

		public OrderItemsRepository(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<OrderItem> AddOrderItem(OrderItem item)
		{
			await _db.OrderItems.AddAsync(item);
			//await _db.SaveChangesAsync();
			return item;
		}

		public async Task<bool> DeleteOrderItemByItemId(Guid itemId)
		{
			IEnumerable<OrderItem> itemsRange = _db.OrderItems.Where(item => item.OrderItemId == itemId);
			_db.OrderItems.RemoveRange(itemsRange);

			return itemsRange.Count() > 0;
		}

		public async Task<decimal> DeleteOrderItemsByOrderId(Guid orderId)
		{
			IEnumerable<OrderItem> items = await GetOrderItemsByOrderId(orderId);
			decimal totalPrice = items.Sum(i => i.TotalPrice);

			_db.RemoveRange(items);

			return totalPrice;
		}

		public Task<List<OrderItem>> GetAllOrderItems()
		{
			return _db.OrderItems.ToListAsync();
		}

		public async Task<OrderItem?> GetOrderItemByOrderItemId(Guid orderItemId)
		{
			return await _db.OrderItems.FindAsync(orderItemId);
		}

		public Task<List<OrderItem>> GetOrderItemsByOrderId(Guid orderId)
		{
			return _db.OrderItems.Where(item => item.OrderId == orderId).ToListAsync();
		}

		public async Task<decimal> UpdateOrderItem(OrderItem item)
		{
			OrderItem? matchingItem = await _db.OrderItems.FirstOrDefaultAsync(temp => temp.OrderItemId == item.OrderItemId && temp.OrderId == item.OrderId);

			if(matchingItem == null)
			{
				throw new KeyNotFoundException();
			}
			decimal deltaPrice = item.TotalPrice - matchingItem.TotalPrice;

			//matchingItem.OrderItemId = item.OrderItemId;
			//matchingItem.OrderId = item.OrderId;
			matchingItem.UnitPrice = item.UnitPrice;
			matchingItem.Quantity = item.Quantity;
			matchingItem.TotalPrice = item.TotalPrice;

			//await _db.SaveChangesAsync();
			return deltaPrice;
		}
	}
}
