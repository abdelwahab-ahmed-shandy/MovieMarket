namespace MovieMart.Services.IServices
{
    public interface ICartService
    {
        Task<List<Cart>> GetCartItemsByUserIdAsync(string userId);
    }
}
