using MediatR;
using Orders.Core.DTO;
using Orders.Core.Events;
using Orders.Core.ServiceContracts.OrderItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.EventHandlers
{
	public class OrderRemovedEventHandler : INotificationHandler<OrderRemovedEvent>
	{
		private readonly IOrderItemGetterService _itemGetterService;
		private readonly IOrderItemDeleterService _itemDeleterService;

		public OrderRemovedEventHandler(IOrderItemGetterService getterService, IOrderItemDeleterService deleterService)
		{
			_itemGetterService = getterService;
			_itemDeleterService = deleterService;
		}
		public async Task Handle(OrderRemovedEvent notification, CancellationToken cancellationToken)
		{
			IEnumerable<OrderItemResponse> orderItemResponses = await _itemGetterService.GetOrderItemsByOrderId(notification.OrderId);

			foreach(OrderItemResponse response in orderItemResponses)
			{
				await _itemDeleterService.DeleteOrderItemByOrderItemId(response.OrderItemId);
			}
		}
	}
}
