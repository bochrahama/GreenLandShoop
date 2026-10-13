//build the interface for the category service
using EcommerceBackend.Entities;
using EcommerceBackend.DTOs;
namespace EcommerceBackend.Services
{
    
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(CategoryCreateDto category);
        Task<bool> UpdateAsync(int id,CategoryCreateDto category);
        Task<bool> DeleteAsync(int id);
    }
}
