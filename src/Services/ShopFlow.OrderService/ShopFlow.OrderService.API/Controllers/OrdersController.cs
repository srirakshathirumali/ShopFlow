using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopFlow.OrderService.Application.DTOs;
using ShopFlow.OrderService.Application.Interfaces;

namespace ShopFlow.OrderService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
                _orderService = orderService;
        }

        
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request)
        {
            var result = await _orderService.CreateOrderAsync(request);
            return CreatedAtAction(
                nameof(GetOrderById), new { id = result.Id }, result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>),StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdersByCustomer(Guid customerId)
        {
            var result = await _orderService
                .GetOrdersByCustomerAsync(customerId);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequestDto request)
        {
            var result = await _orderService.CancelOrderAsync(id, request.Reason);
            return Ok(result);
        }
    }
}
