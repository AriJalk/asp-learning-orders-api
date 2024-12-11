using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.Domain.RepositoryContracts
{
	public interface IUnitOfWork : IDisposable
	{
		IOrdersRepository OrdersRepository { get; }
		IOrderItemsRepository OrderItemsRepository { get; }
		Task<int> SaveAsync();
	}
}
