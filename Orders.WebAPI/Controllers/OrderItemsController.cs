using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Core.Domain.Entities;
using Orders.Core.DTO;
using Orders.Core.ServiceContracts.OrderItems;
using Orders.Core.ServiceContracts.Orders;
using Orders.Infrastructure.DatabaseContexts;

namespace Orders.WebAPI.Controllers
{
	[Route("api/orders/{orderId}/items")]
	[ApiController]
	public class OrderItemsController : ControllerBase
	{
		private readonly ILogger<OrderItemsController> _logger;

		private readonly IOrderItemAdderService _orderItemAdderService;
		private readonly IOrderItemGetterService _orderItemGetterService;
		private readonly IOrderItemUpdaterService _orderItemUpdaterService;
		private readonly IOrderItemDeleterService _orderItemDeleterService;

		private readonly IOrderGetterService _orderGetterService;

		public OrderItemsController(ILogger<OrderItemsController> logger, IOrderItemAdderService adderService, IOrderItemGetterService getterService, IOrderItemUpdaterService updaterService, IOrderItemDeleterService deleterService, IOrderGetterService orderGetterService)
		{
			_logger = logger;

			_orderItemAdderService = adderService;
			_orderItemGetterService = getterService;
			_orderItemUpdaterService = updaterService;
			_orderItemDeleterService = deleterService;
			_orderGetterService = orderGetterService;
		}

		[HttpGet]
		public async Task<ActionResult<List<OrderItemResponse>>> GetOrderItemsByOrderId(Guid orderId)
		{
			_logger.LogInformation($"{nameof(OrderItemsController)}/{nameof(GetOrderItemsByOrderId)}\nGetting order items by order: {orderId}");
			List<OrderItemResponse> orderItemResponses = await _orderItemGetterService.GetOrderItemsByOrderId(orderId);

			return Ok(orderItemResponses);
		}

		[HttpGet("{itemId}")]
		public async Task<ActionResult<OrderItemResponse>> GetOrderItemByOrderItemId(Guid orderId, Guid itemId)
		{
			_logger.LogInformation($"{nameof(OrderItemsController)}/{nameof(GetOrderItemByOrderItemId)}\nGetting order-item by id: {itemId}, order: {orderId}");
			try
			{
				OrderItemResponse response = await _orderItemGetterService.GetOrderItemByOrderItemId(itemId);
				if (response.OrderId == orderId)
				{
					return Ok(response);
				}
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { Message = "Order-item not found.", Details = ex.Message });
			}

			return NotFound(new { Message = "Order-Item not found in order" });
		}

		[HttpPost]
		public async Task<ActionResult<OrderItemResponse>> AddOrderItem(Guid orderId, OrderItemAddRequest orderItemAddRequest)
		{
			_logger.LogInformation($"{nameof(OrderItemsController)}/{nameof(AddOrderItem)}\nAdding Order-Item");
			if (orderId != orderItemAddRequest.OrderId)
			{
				return BadRequest(new { Message = "Mismatch OrderId" });
			}
			try
			{
				OrderItemResponse response = await _orderItemAdderService.AddOrderItem(orderItemAddRequest);
				return CreatedAtAction(nameof(GetOrderItemByOrderItemId), new { orderId = response.OrderId, itemId = response.OrderItemId }, response);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { Message = "Order not found", Details = ex.Message });
			}
		}

		[HttpDelete("{itemId}")]
		public async Task<IActionResult> DeleteOrderItem(Guid orderId, Guid itemId)
		{
			_logger.LogInformation($"{nameof(OrderItemsController)}/{nameof(DeleteOrderItem)}\Deleting Order-Item");
			try
			{
				OrderItemResponse itemResponse = await _orderItemGetterService.GetOrderItemByOrderItemId(itemId);
				if (itemResponse.OrderId != orderId)
				{
					return BadRequest(new { Message = "Order-Id mismatch" });
				}
				bool result = await _orderItemDeleterService.DeleteOrderItemByOrderItemId(itemId);
				if (result)
				{
					return NoContent();
				}
				throw new ApplicationException();
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { Message = "Order-item not found.", Details = ex.Message });
			}
		}
			
		[HttpPut("{itemId}")]
		public async Task<ActionResult<OrderItemResponse>> UpdateOrderItem(Guid orderId, Guid itemId, OrderItemUpdateRequest updateRequest)
		{
			_logger.LogInformation($"{nameof(OrderItemsController)}/{nameof(UpdateOrderItem)}\Updating Order-Item");
			if (updateRequest.OrderItemId != itemId)
			{
				return BadRequest("Mismatch Item-ID");
			}
			if (updateRequest.OrderId != orderId)
			{
				return BadRequest("Mismatch Order-ID");
			}
			try
			{
				OrderItemResponse itemResponse = await _orderItemUpdaterService.UpdateOrderItem(updateRequest);
				return Ok(itemResponse);

			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(new { Message = "Order-item not found.", Details = ex.Message });
			}
		}
	}
}
