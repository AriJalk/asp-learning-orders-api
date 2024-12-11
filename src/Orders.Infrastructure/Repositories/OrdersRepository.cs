using Microsoft.EntityFrameworkCore;
using Orders.Core.Domain.Entities;
using Orders.Core.Domain.RepositoryContracts;
using Orders.Infrastructure.DatabaseContexts;
using System.Linq.Expressions;


namespace Orders.Infrastructure.Repositories
{
	public class OrdersRepository : IOrdersRepository
	{
		private readonly ApplicationDbContext _db;

		public OrdersRepository(ApplicationDbContext db)
		{
			_db = db;
		}

		public async Task<Order> AddOrder(Order order)
		{
			await _db.Orders.AddAsync(order);
			//await _db.SaveChangesAsync();
			return order;
		}

		public async Task<bool> DeleteOrder(Guid orderId)
		{
			IEnumerable<Order> orderRange = _db.Orders.Where(order => order.OrderId == orderId);
			_db.Orders.RemoveRange(orderRange);

			return orderRange.Count() > 0;
		}

		public async Task<List<Order>> GetAllOrders()
		{
			return await _db.Orders.ToListAsync();
		}

		public async Task<List<Order>> GetFilteredOrders(Expression<Func<Order, bool>> predicate)
		{
			return await _db.Orders.Where(predicate).ToListAsync();
		}

		public async Task<long> GetNextSequenceNumber()
		{
			SequenceNumber?  sequenceNumber = await _db.SequenceNumber.FirstOrDefaultAsync(s => s.Year == DateTime.Today.Year);
			if (sequenceNumber == null)
			{
				sequenceNumber = new SequenceNumber()
				{
					Year = DateTime.Today.Year,
					NextSequenceNumber = 0,
				};
				//Create current year in table and initialize
				await _db.SequenceNumber.AddAsync(sequenceNumber);
			}
			return ++sequenceNumber.NextSequenceNumber;
		}

		public async Task<Order?> GetOrderByOrderID(Guid orderId)
		{
			return await _db.Orders.FirstOrDefaultAsync(order => order.OrderId == orderId);
		}

		public async Task<Order> UpdateOrder(Order order)
		{
			Order? matchingOrder = await _db.Orders.FirstOrDefaultAsync(temp => temp.OrderId == order.OrderId);
			if (matchingOrder == null)
			{
				return order;
			}

			matchingOrder.OrderDate = order.OrderDate;
			matchingOrder.OrderId = order.OrderId;
			matchingOrder.OrderNumber = order.OrderNumber;
			matchingOrder.CustomerName = order.CustomerName;
			matchingOrder.TotalAmount = order.TotalAmount;

			//await _db.SaveChangesAsync();
			return matchingOrder;
		}


	}
}
