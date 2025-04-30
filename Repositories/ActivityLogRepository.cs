namespace MovieMart.Repositories
{
    public class ActivityLogRepository : Repository<ActivityLog>, IActivityLogRepository
    {
        public ActivityLogRepository(MovieMarketDbContext movieMarketDbContext)
            : base(movieMarketDbContext)
        {
        }
    }
}
