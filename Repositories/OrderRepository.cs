namespace MovieMart.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(MovieMarketDbContext movieMarketDbContext) : base(movieMarketDbContext)
        {

        }
    }
}