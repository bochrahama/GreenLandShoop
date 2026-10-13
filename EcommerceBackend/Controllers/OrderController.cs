using Microsoft.AspNetCore.Mvc;
using EcommerceBackend.Services;
using EcommerceBackend.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace EcommerceBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        private string GetUserId()
            => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto checkoutDto)
        {
            try
            {
                var order = await _orderService.CheckoutAsync( GetUserId(),checkoutDto);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]      
        public async Task<IActionResult> GetMyOrders() => Ok(await _orderService.GetOrderForUserAsync(GetUserId()));
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(GetUserId(), id);
            if (order is null)
                return NotFound();
            return Ok(order);
        }
    }
}
