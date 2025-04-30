

namespace MovieMart.Repositories
{
    public class TvSeriesRepository : Repository<TvSeries>, ITvSeriesRepository
    {
        public TvSeriesRepository(MovieMarketDbContext movieMarketDbContext) : base(movieMarketDbContext)
        {

        }
    }
}
