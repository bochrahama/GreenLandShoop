//build the LoginDto class to represent the data needed for user login.
namespace EcommerceBackend.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
