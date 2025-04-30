using System.Linq.Expressions;

namespace MovieMart.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;

        public CartService(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<List<Cart>> GetCartItemsByUserIdAsync(string userId)
        {
            return await _cartRepository.Get(
                filter: c => c.ApplicationUserId == userId && c.CartStatus == CartStatus.InCart,
                includes: new Expression<Func<Cart, object>>[]
                {
            c => c.Cinema,
            c => c.Movie
                },
                tracked: false
            ).ToListAsync();

        }
    }
}
