using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.Events
{
	public class OrderItemChangedEvent : INotification
	{
		public Guid OrderId { get; set; }
		public decimal DeltaAmount { get; set; }
	}
}
