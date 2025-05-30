namespace MovieMart.Repositories
{
    public class SentEmailRepository : Repository<SentEmail>, ISentEmailRepository
    {
        public SentEmailRepository(MovieMarketDbContext movieMarketDbContext) : base(movieMarketDbContext)
        {

        }
    }
}
