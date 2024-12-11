using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.Domain.Entities
{
	public class Order
	{
		[Key]
		public Guid OrderId { get; set; }

		[Required]
		public string? OrderNumber { get; set; }

		[Required]
		public string? CustomerName { get; set; }

		[Required]
		public DateTime? OrderDate { get; set; }

		[Required]
		[Range(typeof(decimal), "0", "79228162514264337593543950335")]
		[Column(TypeName = "decimal")]
		public decimal TotalAmount { get; set; }

		public static Order CopyOrder(Order otherOrder)
		{
			Order order = new Order() {
				OrderId = otherOrder.OrderId,
				OrderNumber = otherOrder.OrderNumber,
				CustomerName = otherOrder.CustomerName,
				OrderDate = otherOrder.OrderDate,
				TotalAmount = otherOrder.TotalAmount,
			};
			return order;
		}
	}
}
