//build the service layer for the category entity, which will be used to handle business logic and interact with the repository layer. The service layer will provide methods for creating, reading, updating, and deleting categories.
using EcommerceBackend.DTOs;
using EcommerceBackend.Entities;
using EcommerceBackend.Repositories;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.VisualBasic;

namespace EcommerceBackend.Services
{
    public class CategoryService : ICategoryService
    {

        private readonly IGenericRepository<Category, int> _repo;
        public CategoryService(IGenericRepository<Category, int> repo) { _repo = repo; }
        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _repo.GetAllAsync();

            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            });
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category is null) return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryDto> CreateAsync(CategoryCreateDto createDto)
        {
       
            var categoryEntity = new Category
            {
                Name = createDto.Name,
                Description = createDto.Description
            };

            await _repo.AddAsync(categoryEntity);
            await _repo.SaveChangesAsync(); 

            return new CategoryDto
            {
                Id = categoryEntity.Id,
                Name = categoryEntity.Name,
                Description = categoryEntity.Description
            };
        }

        public async Task<bool> UpdateAsync(int id, CategoryCreateDto updateDto)
        {
            var existingCategory = await _repo.GetByIdAsync(id);
            if (existingCategory is null) return false;

            existingCategory.Name = updateDto.Name;
            existingCategory.Description = updateDto.Description;

            _repo.Update(existingCategory);
            return await _repo.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category is null) return false;

            _repo.Remove(category);
            return await _repo.SaveChangesAsync();
        }
    }
}
        
