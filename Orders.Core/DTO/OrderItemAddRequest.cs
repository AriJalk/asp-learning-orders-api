using Orders.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.DTO
{
	public class OrderItemAddRequest
	{
		public Guid OrderId { get; set; } = Guid.Empty;

		[Required]
		[MaxLength(50)]
		public string? ProductName { get; set; }

		[Range(1, Int32.MaxValue)]
		public int Quantity { get; set; }

		[Range(0, 1000000)]
		public decimal UnitPrice { get; set; }

		[Range(0, 100000000)]
		public decimal TotalPrice { get; set; }

		public OrderItem ToOrderItem()
		{
			return new OrderItem()
			{
				OrderId = this.OrderId,
				ProductName = this.ProductName,
				Quantity = this.Quantity,
				UnitPrice = this.UnitPrice,
				TotalPrice = this.TotalPrice,
			};
		}
	}
}
