using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Orders.Core.Domain.RepositoryContracts;
using Orders.Infrastructure.DatabaseContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Infrastructure.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ApplicationDbContext _context;
		public IOrdersRepository OrdersRepository { get; }
		public IOrderItemsRepository OrderItemsRepository { get; }

		public UnitOfWork(ApplicationDbContext context)
		{
			_context = context;
			OrdersRepository = new OrdersRepository(context);
			OrderItemsRepository = new OrderItemsRepository(context);
		}
		public void Dispose()
		{
			_context.Dispose();
		}

		public async Task<int> SaveAsync()
		{
			using IDbContextTransaction? transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				int rowsAffected = await _context.SaveChangesAsync();
				await transaction.CommitAsync();
				return rowsAffected;
			}
			catch(Exception ex) 
			{
				await transaction.RollbackAsync();
				throw new ApplicationException("Can't commit transaction to database", ex);
			}
		}
	}
}
