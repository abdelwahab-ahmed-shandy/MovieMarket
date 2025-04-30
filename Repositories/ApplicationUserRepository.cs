using MovieMart.Repositories.IRepositories;

namespace MovieMart.Repositories
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        public ApplicationUserRepository(MovieMarketDbContext movieMartDbContext) : base(movieMartDbContext)
        {
        }
    }
}
