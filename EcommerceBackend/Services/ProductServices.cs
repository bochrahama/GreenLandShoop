//Update Product servise by adding a new method to get products with low stock and a method to adjust stock by a given delta. The AdjustStockAsync method will check if the resulting stock is negative and throw an exception if it is. The GetLowStockAsync method will return products that have a quantity less than or equal to a specified threshold.
using EcommerceBackend.Data;
using EcommerceBackend.DTOs;
using EcommerceBackend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace EcommerceBackend.Services
{
    public class ProductServices : IProductService
    {
        private readonly AppDbContext _context;

        public ProductServices(AppDbContext context)
        {
            _context = context;
        }
        private static ProductDto MapToDto(Product p) => new()
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            ProductDescription = p.ProductDescription,
            ProductPrice = p.ProductPrice,
            ProductQuantity = (decimal)p.ProductQuantity!,
            CategoryName = p.ProductCategory?.Name ?? string.Empty,
            ImageUrls = p.ProductImages.Select(i => i.Url).ToList()
        };
        public async Task<IEnumerable<ProductDto>> GetAllAsync(string? search, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Products
        .Include(p => p.ProductCategory)
        .Include(p => p.ProductImages)
        .Where(p => p.ProductIsActive)
        .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.ProductName.Contains(search));
            if (categoryId.HasValue)
                query = query.Where(p => p.ProductCategoryId == categoryId);
            if (minPrice.HasValue)
                query = query.Where(p => p.ProductPrice >= minPrice);
            if (maxPrice.HasValue)
                query = query.Where(p => p.ProductPrice <= maxPrice);

            var products = await query.ToListAsync();
            return products.Select(MapToDto);
        }
        // the method below is used to get a product by its id and include its category and images
        public async Task<ProductDto?> GetByIdAsync(int id)
        {
           var product = await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductImages)
                // filter the product by its id and check if it is active
                .FirstOrDefaultAsync(p => p.ProductId == id && p.ProductIsActive);
            return product is null ? null : MapToDto(product);
        }
        //the method below is used to create a new product 
        // it takes a product object as a parameter and adds it to the database
        public async Task<ProductDto> CreateAsync(ProductCreateDto dto)
        {
            var product = new Product
            {
                ProductName = dto.ProductName,
                ProductDescription = dto.ProductDescription,
                ProductPrice = dto.ProductPrice,
                ProductQuantity = dto.ProductQuantity,
                ProductCategoryId = dto.ProductCategoryId,
                ProductIsActive = true
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return MapToDto(product);
        
        }
        public async Task<bool> UpdateAsync(int id, ProductCreateDto dto)
        {
            // find the existing product by its id by using the FindAsync method of the DbContext
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null || !existingProduct.ProductIsActive)
            {
                return false;
            }
            // update the existing product with the new values
            // update the existing product with the new values
            existingProduct.ProductName = dto.ProductName;
            existingProduct.ProductDescription = dto.ProductDescription;
            existingProduct.ProductPrice = dto.ProductPrice;
            existingProduct.ProductQuantity = dto.ProductQuantity;
            existingProduct.ProductCategoryId = dto.ProductCategoryId;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null || !existingProduct.ProductIsActive)
            {
                return false;
            }
            // mark the product as inactive instead of deleting it from the database
            existingProduct.ProductIsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
        // the method below is used to reduce the stock of a product by its id and quantity
        //by checking if the product exists and is active and has enough quantity
        public async Task<bool> ReduceStockAsync(int productId, decimal quantity)
        {
            var existingProduct = await _context.Products.FindAsync(productId);
            if (existingProduct == null || !existingProduct.ProductIsActive || existingProduct.ProductQuantity < quantity)
            {
                return false;
            }// reduce the stock of the product by the quantity and save the changes to the database
            existingProduct.ProductQuantity -= quantity;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ProductDto?> AdjustStockAsync(int id, int delta)
        {
            var product = await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.ProductIsActive);

            if (product is null) return null;

            if (product.ProductQuantity + delta < 0)
                throw new InvalidOperationException("Resulting stock cannot be negative.");

            product.ProductQuantity += delta;
            await _context.SaveChangesAsync();
            return MapToDto(product);
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockAsync(int threshold)
        {
            var products = await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductImages)
                .Where(p => p.ProductIsActive && p.ProductQuantity <= threshold)
                .OrderBy(p => p.ProductQuantity)
                .ToListAsync();

            return products.Select(MapToDto);
        }
    }
}
