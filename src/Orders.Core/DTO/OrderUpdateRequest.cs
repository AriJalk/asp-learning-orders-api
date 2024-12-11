using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.DTO
{
	public class OrderUpdateRequest
	{
		public Guid OrderId { get; set; }

		[Required]
		public string? OrderNumber { get; set; }
		[Required]
		public string? CustomerName { get; set; }

		public DateTime OrderDate { get; set; }

		[Range(typeof(decimal), "0", "79228162514264337593543950335")]
		public decimal TotalAmount { get; set; }
	}

	public static class OrderUpdateExtensionMethods
	{
		public static OrderUpdateRequest ToOrderUpdateRequest(this OrderResponse orderResponse)
		{
			return new OrderUpdateRequest()
			{
				CustomerName = orderResponse.CustomerName,
				OrderNumber = orderResponse.OrderNumber,
				OrderDate = orderResponse.OrderDate,
				TotalAmount = orderResponse.TotalAmount,
				OrderId = orderResponse.OrderId,
			};
		}
	}
}
