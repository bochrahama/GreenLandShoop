//build the interface for the product service
using EcommerceBackend.DTOs;
using EcommerceBackend.Entities;
using Microsoft.AspNetCore.Mvc;
namespace EcommerceBackend.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice);
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto> CreateAsync(ProductCreateDto dto);

        Task<bool> UpdateAsync(int id , ProductCreateDto dto);
        Task<bool> DeleteAsync(int id);
        
        Task <bool> ReduceStockAsync (int productId, decimal quantity);
        Task<ProductDto?> AdjustStockAsync(int id, int delta);
        Task<IEnumerable<ProductDto>> GetLowStockAsync(int threshold);
    }

}
