namespace MovieMart.Repositories
{
    public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
    {
        public OrderItemRepository(MovieMarketDbContext movieMarketDbContext)
            : base(movieMarketDbContext)
        {

        }
    }
}