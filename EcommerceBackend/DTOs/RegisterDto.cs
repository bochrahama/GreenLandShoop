using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.DTOs
{
    public class RegisterDto
    {
        [Required,MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        [Required,EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required,MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}
