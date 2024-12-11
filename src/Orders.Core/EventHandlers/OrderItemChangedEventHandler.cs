using MediatR;
using Orders.Core.DTO;
using Orders.Core.Events;
using Orders.Core.ServiceContracts.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Core.EventHandlers
{
	public class OrderItemChangedEventHandler : INotificationHandler<OrderItemChangedEvent>
	{
		private readonly IOrderGetterService _getterService;
		private readonly IOrderUpdaterService _updaterService;

		public OrderItemChangedEventHandler(IOrderGetterService getterService, IOrderUpdaterService updaterService)
		{
			_getterService = getterService;
			_updaterService = updaterService;
		}

		public async Task Handle(OrderItemChangedEvent notification, CancellationToken cancellationToken)
		{
			OrderResponse? response = await _getterService.GetOrderByOrderId(notification.OrderId);
			if (response == null) 
			{
				throw new Exception("Order not found");
			}

			OrderUpdateRequest orderUpdateRequest = response.ToOrderUpdateRequest();
			orderUpdateRequest.TotalAmount += notification.DeltaAmount;
			await _updaterService.UpdateOrder(orderUpdateRequest);
		}
	}
}
