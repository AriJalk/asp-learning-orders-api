using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.Domain.Entities
{
	public class OrderItem
	{
		[Key]
		public Guid OrderItemId { get; set; }

		public Guid OrderId { get; set; }

		[Required]
		[MaxLength(50)]
		public string? ProductName { get; set; }

		[Range(1, Int32.MaxValue)]
		public int Quantity { get; set; }

		[Range(0, 1000000)]
		[Column(TypeName = "decimal")]
		public decimal UnitPrice { get; set; }

		[Range(0, 100000000)]
		[Column(TypeName = "decimal")]
		public decimal TotalPrice { get; set; }
	}
}
