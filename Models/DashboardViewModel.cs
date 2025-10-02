namespace DisasterAlleviationFoundation.Models
{
    public class DashboardViewModel
    {
        public User CurrentUser { get; set; } = null!;
        public List<ActivityItem> RecentActivities { get; set; } = new();
        public DashboardStats Stats { get; set; } = new();
    }

    public class ActivityItem
    {
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = string.Empty; // success, info, warning, error
    }

    public class DashboardStats
    {
        public int TotalVolunteers { get; set; }
        public decimal TotalDonations { get; set; }
        public int ActiveEmergencies { get; set; }
        public int CommunitiesHelped { get; set; }
    }
}