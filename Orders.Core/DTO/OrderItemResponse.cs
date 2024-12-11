using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orders.Core.Domain.Entities;

namespace Orders.Core.DTO
{
	public class OrderItemResponse
	{
		public Guid OrderItemId { get; set; }
		public Guid OrderId { get; set; }
		public string? ProductName { get; set; }
		public int Quantity { get; set; }
		public decimal UnitPrice { get; set; }
		public decimal TotalPrice { get; set; }
	}

	public static class OrderItemExtensionMethods
	{
		public static OrderItemResponse ToResponse(this OrderItem item)
		{
			return new OrderItemResponse()
			{
				OrderId = item.OrderId,
				OrderItemId = item.OrderItemId,
				ProductName = item.ProductName,
				Quantity = item.Quantity,
				UnitPrice = item.UnitPrice,
				TotalPrice = item.TotalPrice,
			};
		}
	}
}
