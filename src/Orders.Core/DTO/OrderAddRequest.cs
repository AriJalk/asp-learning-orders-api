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
	public class OrderAddRequest
	{
		[Required]
		public string? CustomerName { get; set; }

		public List<OrderItemAddRequest> OrderItems { get; set; } = new List<OrderItemAddRequest>();
	}


}
