using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Orders.Core.Domain.Entities;

namespace Orders.Core.DTO
{
	public class OrderResponse
	{
		public Guid OrderId { get; set; }
		public string?	 OrderNumber { get; set; }
		public string? CustomerName { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal TotalAmount { get; set; }

		public List<OrderItemResponse> OrderItems { get; set; } = new List<OrderItemResponse>();
	}

	public static class OrderResponseExtensions
	{
		public static OrderResponse ToOrderResponse(this Order order)
		{
			return new OrderResponse()
			{
				OrderId = order.OrderId,
				OrderNumber = order.OrderNumber,
				CustomerName = order.CustomerName,
				OrderDate = order.OrderDate.Value,
				TotalAmount = order.TotalAmount,
			};
		}
	}
}
