using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Core.Domain.Entities;
using Orders.Core.Domain.RepositoryContracts;
using Orders.Core.DTO;
using Orders.Core.ServiceContracts.Orders;
using Orders.Infrastructure.DatabaseContexts;

namespace Orders.WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OrdersController : ControllerBase
	{
		private readonly IOrderAdderService _orderAdderService;
		private readonly IOrderGetterService _orderGetterService;
		private readonly IOrderFilterService _orderFilterService;
		private readonly IOrderUpdaterService _orderUpdaterService;
		private readonly IOrderDeleterService _orderDeleterService;

		private readonly ILogger<OrdersController> _logger;

		public OrdersController(ILogger<OrdersController> logger, IOrderAdderService adderService, IOrderGetterService getterService, IOrderFilterService filterService, IOrderUpdaterService updaterService, IOrderDeleterService deleterService)
		{
			_logger = logger;

			_orderAdderService = adderService;
			_orderGetterService = getterService;
			_orderFilterService = filterService;
			_orderUpdaterService = updaterService;
			_orderDeleterService = deleterService;
		}

		// GET: api/Orders
		[HttpGet]
		public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrders()
		{
			IEnumerable<OrderResponse> responses = await _orderGetterService.GetAllOrders();

			return Ok(responses);
		}

		// GET: api/Orders/5
		[HttpGet("{id}")]
		public async Task<ActionResult<OrderResponse>> GetOrderById(Guid id)
		{
			_logger.LogInformation($"{nameof(OrdersController)}/{nameof(GetOrderById)}\nGetting order {id}");
			try
			{
				OrderResponse order = await _orderGetterService.GetOrderByOrderId(id);
				return Ok(order);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(ex.Message);
			}
		}


		// POST: api/Orders
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		[HttpPost]
		public async Task<ActionResult<OrderResponse>> AddOrder(OrderAddRequest orderAddRequest)
		{
			_logger.LogInformation($"{nameof(OrdersController)}/{nameof(AddOrder)}\nAdding order");
			try
			{
				OrderResponse response = await _orderAdderService.AddOrder(orderAddRequest);
				_logger.LogInformation($"{nameof(OrdersController)}/{nameof(AddOrder)}\nAdd successful");
				return CreatedAtAction(nameof(GetOrderById), new { id = response.OrderId }, response);
			}
			catch
			{
				throw;
			}
		}

		[HttpPut("{id}")]
		public async Task<ActionResult<OrderResponse>> UpdateOrder(Guid id, OrderUpdateRequest orderUpdateRequest)
		{
			_logger.LogInformation($"{nameof(OrdersController)}/{nameof(UpdateOrder)}\nUpdating order {id}");
			if (id != orderUpdateRequest.OrderId)
			{
				_logger.LogInformation($"{nameof(OrdersController)}/{nameof(UpdateOrder)}\nUpdate failure");
				return BadRequest(new { Message = "Mismatch id" });
			}
			try
			{
				OrderResponse orderResponse = await _orderUpdaterService.UpdateOrder(orderUpdateRequest);
				_logger.LogInformation($"{nameof(OrdersController)}/{nameof(UpdateOrder)}\nUpdate successful");
				return Ok(orderResponse);

			}
			catch
			{
				throw;
			}
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteOrder(Guid id)
		{
			_logger.LogInformation($"{nameof(OrdersController)}/{nameof(DeleteOrder)}\nDeleting order {id}");
			bool result = await _orderDeleterService.DeleteOrderByOrderId(id);

			if (result)
			{
				_logger.LogInformation($"{nameof(OrdersController)}/{nameof(DeleteOrder)}\nDelete successful");
				return NoContent();
			}
			_logger.LogInformation($"{nameof(OrdersController)}/{nameof(DeleteOrder)}\nOrder not found");
			return NotFound();
		}
	}
}
