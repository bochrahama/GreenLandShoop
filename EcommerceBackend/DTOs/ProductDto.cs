namespace EcommerceBackend.DTOs
{
    public class ProductDto
    {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string? ProductDescription { get; set; }
            public decimal ProductPrice { get; set; }
            public decimal ProductQuantity { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public List<string> ImageUrls { get; set; } = new();
        }
    public class ProductCreateDto
    {
        [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(200)]

public string ProductName { get; set; } = string.Empty;
        public string? ProductDescription { get; set; }
        [System.ComponentModel.DataAnnotations.Range(0.01, 100000)]
        public decimal ProductPrice { get; set; }
        [System.ComponentModel.DataAnnotations.Range(0, int.MaxValue)]
        public int ProductQuantity { get; set; }
        public int ProductCategoryId { get; set; }
    }
}
