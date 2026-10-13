using System.ComponentModel.DataAnnotations;
using System.Text;
//the pojo class for the checkout process, containing shipping address and name on card
namespace EcommerceBackend.DTOs
{
    public class CheckoutDto
    {
        [Required]
        public string ShippingAddress { get; set; } = string.Empty;
        public string NameOnCard { get; set; } = string.Empty;
    }
}
