using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MovieMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class HomeController : Controller
    {
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly IMovieRepository _movieRepository;
        private readonly ITvSeriesRepository _tvSeriesRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly IActivityLogRepository _activityLogRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(IApplicationUserRepository applicationUserRepository,
                                                                IMovieRepository movieRepository,
                                                                    ITvSeriesRepository tvSeriesRepository,
                                                                        IOrderRepository orderRepository,
                                                                          IActivityLogRepository activityLogRepository,
                                                                            UserManager<ApplicationUser> userManager,
                                                                            ISubscriberRepository subscriberRepository)
        {
            _applicationUserRepository = applicationUserRepository;
            _movieRepository = movieRepository;
            _orderRepository = orderRepository;
            _tvSeriesRepository = tvSeriesRepository;
            _activityLogRepository = activityLogRepository;
            _userManager = userManager;
            _subscriberRepository = subscriberRepository;
        }

        public IActionResult Index()
        {
            var totalUsers = _userManager.Users.ToList();

            var adminUsers = totalUsers.Count(u => _userManager.GetRolesAsync(u).Result.Contains("Admin"));
            var superAdminUsers = totalUsers.Count(u => _userManager.GetRolesAsync(u).Result.Contains("SuperAdmin"));

            var dashBord = new DashboardViewModel()
            {
                TotalUsers = totalUsers.Count,
                TotalAdmins = adminUsers,
                TotalSuperAdmins = superAdminUsers,

                TotalMovies = _movieRepository.Get().AsNoTracking().Count(),
                TotalTvSeries = _tvSeriesRepository.Get().AsNoTracking().Count(),
                TotalOrders = _orderRepository.Get().AsNoTracking().Count(),
                PendingOrders = _orderRepository.Get().AsNoTracking().Count(o => o.Status == OrderStatus.Pending),

                TotalSubscribers = _subscriberRepository.Get().Count(),

                RecentSubscribers = _subscriberRepository.Get()
                         .OrderByDescending(s => s.SubscribedAt)
                         .Take(5)
                         .ToList(),

                // todo : here 
                //NewUsersThisMonth = _userManager.Users.Count(u=>u.Created),
                //HighPriorityOrders   = _orderRepository.Get().Count(o => o.Pro),

                // NewContentToday 
                RecentActivities = _activityLogRepository.Get()
                                    .Include(a => a.User)
                                    .OrderByDescending(x => x.Timestamp)
                                    .Take(5)
                                    .ToList()
            };

            return View(dashBord);
        }



    }

}
