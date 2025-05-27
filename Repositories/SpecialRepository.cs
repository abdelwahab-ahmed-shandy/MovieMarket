namespace MovieMart.Repositories
{
    public class SpecialRepository : Repository<Special>, ISpecialRepository
    {
        public SpecialRepository(MovieMarketDbContext movieMarketDbContext)
        : base(movieMarketDbContext)
        {

        }
    }
}
