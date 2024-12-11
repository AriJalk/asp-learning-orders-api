using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.Events
{
	public class OrderRemovedEvent : INotification
	{
		public Guid OrderId { get; set; }
	}
}
