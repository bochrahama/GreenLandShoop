using Microsoft.AspNetCore.Mvc;
using EcommerceBackend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using EcommerceBackend.DTOs;
namespace EcommerceBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;
        public CartController(ICartService service)
        {
            _service = service;
        }
        private string GetUserId()
          => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<IActionResult> GetCart() => Ok(await _service.GetCartAsync(GetUserId()));
        [HttpPost("items")]
        public async Task<IActionResult> AddItemToCart([FromBody] CartItemDto cartItemDto)
        {
            try
            {
                var cart =
                await _service.AddItemAsync(GetUserId(),
                cartItemDto);
                return Ok(cart);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }
        [HttpPut("items/{cartItemId:int}")]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] CartItemDto cartItemDto)
        {
                        
                var update = await _service.UpdateQuantityAsync(GetUserId(), cartItemId, cartItemDto.Quantity);
                
                if(!update)
                {
                    return NotFound();
                }
                return NoContent();

        }
        [HttpDelete("items/{productId:int}")]
        public async Task<IActionResult> RemoveCartItem(int productId)
        {
          
           
                var remove = await _service.RemoveItemFromCartAsync(GetUserId(), productId);
                if (remove)
                {
                    return NoContent();
                }
                return NotFound();
           
        }
        [HttpDelete("items")]
        public async Task<IActionResult> ClearCartAsync()
        {
            await _service.ClearCartAsync(GetUserId());
            return NoContent();
    }
    }
}
