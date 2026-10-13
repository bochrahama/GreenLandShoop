using EcommerceBackend.Entities;
namespace EcommerceBackend.Services
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(AppUser user);
    }
}
