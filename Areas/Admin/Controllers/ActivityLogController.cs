using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MovieMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ActivityLogController : Controller
    {
        private readonly IActivityLogRepository _activityLogRepository;
        private readonly IApplicationUserRepository _applicationUserRepository;
        public ActivityLogController(IActivityLogRepository activityLogRepository,
                                            IApplicationUserRepository applicationUserRepository)
        {
            _activityLogRepository = activityLogRepository;
            _applicationUserRepository = applicationUserRepository;
        }

    }
}
