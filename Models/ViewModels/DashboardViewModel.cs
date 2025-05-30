namespace MovieMart.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalSuperAdmins { get; set; }
        public int TotalMovies { get; set; }
        public int TotalTvSeries { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public IEnumerable<ActivityLog> RecentActivities { get; set; }


        public int NewUsersThisMonth { get; set; }
        public int NewContentToday { get; set; }
        public int HighPriorityOrders { get; set; }


        public int TotalSubscribers { get; set; }
        public IEnumerable<Subscriber> RecentSubscribers { get; set; }

    }
}
