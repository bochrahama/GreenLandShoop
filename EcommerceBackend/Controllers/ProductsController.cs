using Microsoft.AspNetCore.Mvc;
using EcommerceBackend.Services;
using EcommerceBackend.Entities;
using EcommerceBackend.DTOs;
using Microsoft.AspNetCore.Authorization;
namespace EcommerceBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int? categoryId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            var products = await _productService.GetAllAsync(search, categoryId, minPrice, maxPrice);
            return Ok(products);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ProductCreateDto dto)
        {
            var createdProductDto = await _productService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdProductDto.ProductId }, createdProductDto);
        }
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, ProductCreateDto dto)
        {

            var updatedProduct = await _productService.UpdateAsync(id, dto);
            if (!updatedProduct)
            {
                return NotFound();
            }
            return Ok(updatedProduct);
        }
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _productService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }
        [HttpPost("{id:int}/stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdjustStock(int id, StockAdjustmentDto dto)
        {
            try
            {
                var result = await _productService.AdjustStockAsync(id, dto.Quantity);
                if (result is null) return NotFound();
                return Ok(new { result.ProductId, result.ProductQuantity, dto.Reason });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("low-stock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 5)
            => Ok(await _productService.GetLowStockAsync(threshold));
    }
}

