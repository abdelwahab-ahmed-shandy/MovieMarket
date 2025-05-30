namespace MovieMart.Repositories
{

    public class SubscriberRepository : Repository<Subscriber>, ISubscriberRepository
    {
        public SubscriberRepository(MovieMarketDbContext movieMarketDbContext)
            : base(movieMarketDbContext)
        {

        }
    }
}
