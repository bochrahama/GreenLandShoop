using Microsoft.AspNetCore.Identity;
namespace EcommerceBackend.Entities
{
    //build AppUser Class its inherit from IdentityUser class and add some properties like Age and Address  
    public class AppUser : IdentityUser
    {

        public string FullName { get; set; } = string.Empty;
        private int _Age;
        public string Address { get; set; } = string.Empty;

        public int Age
        {
            get { return _Age; }
            set
            {
                if (value <15 || value > 90)
                {
                    throw new ArgumentOutOfRangeException("Age must be between 15 and 90.");
                }
                _Age = value;
            }
        }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public AppUser()
        {
        }
    }
}
