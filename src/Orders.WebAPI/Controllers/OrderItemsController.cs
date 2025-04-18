﻿using System;
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

		//private readonly IOrderGetterService _orderGetterService;

		public OrderItemsController(ILogger<OrderItemsController> logger, IOrderItemAdderService adderService, IOrderItemGetterService getterService, IOrderItemUpdaterService updaterService, IOrderItemDeleterService deleterService)
		{
			_logger = logger;

			_orderItemAdderService = adderService;
			_orderItemGetterService = getterService;
			_orderItemUpdaterService = updaterService;
			_orderItemDeleterService = deleterService;
		}

		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<ActionResult<List<OrderItemResponse>>> GetOrderItemsByOrderId(Guid orderId)
		{
			_logger.LogInformation($"{nameof(OrderItemsController)}/{nameof(GetOrderItemsByOrderId)}\nGetting order items by order: {orderId}");
			List<OrderItemResponse> orderItemResponses = await _orderItemGetterService.GetOrderItemsByOrderId(orderId);

			return Ok(orderItemResponses);
		}

		[HttpGet("{itemId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
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
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
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
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteOrderItem(Guid orderId, Guid itemId)
		{
			_logger.LogInformation($"{nameof(OrderItemsController)}/{nameof(DeleteOrderItem)}\nDeleting Order-Item");
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
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<OrderItemResponse>> UpdateOrderItem(Guid orderId, Guid itemId, OrderItemUpdateRequest updateRequest)
		{
			_logger.LogInformation($"{nameof(OrderItemsController)}/{nameof(UpdateOrderItem)}\nUpdating Order-Item");
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
