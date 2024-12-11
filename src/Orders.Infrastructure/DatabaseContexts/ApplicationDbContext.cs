using Microsoft.EntityFrameworkCore;
using Orders.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Infrastructure.DatabaseContexts
{
	public class ApplicationDbContext : DbContext
	{
		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderItem> OrderItems { get; set; }
		public DbSet<SequenceNumber> SequenceNumber { get; set; }


		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{

		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<SequenceNumber>().HasData(new SequenceNumber() {Id = 1});
			modelBuilder.HasSequence<long>("OrderSequence");
		}


		public async Task<long> sp_GetNextSequenceNumber()
		{
			long result = Database.SqlQueryRaw<long>("EXECUTE [dbo].[GetNextSequenceNumber]").AsEnumerable().FirstOrDefault();
			return result;
		}
	}
	public class SequenceNumber
	{
		[Key]
		public short Id {  get; set; }
		public long NextSequenceNumber { get; set; }
	}
}
