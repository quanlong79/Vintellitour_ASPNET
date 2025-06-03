namespace Vintellitour_Framework.ViewModels
{
    public class DashboardViewModel
    {
        public List<ProvinceEngagementViewModel> ProvinceEngagements { get; set; }
        public UserPostStatusViewModel UserPostStatus { get; set; }
        public List<PostStatsByMonthViewModel> PostStatsByMonth { get; set; }
        public List<MonthlyRevenueViewModel> MonthlyRevenue { get; set; }
        public List<YearlyRevenueViewModel> YearlyRevenue { get; set; }
    }
    public class ProvinceEngagementViewModel
    {
        public int ProvinceGid { get; set; }
        public string ProvinceName { get; set; }
        public int TotalEngagement { get; set; }
    }

    public class UserPostStatusViewModel
    {
        public int UsersWithPosts { get; set; }
        public int UsersWithoutPosts { get; set; }
    }

    public class PostStatsByMonthViewModel
    {
        public int Month { get; set; }
        public int PostCount { get; set; }
    }

    public class MonthlyRevenueViewModel
    {
        public int Month { get; set; }      // Tháng (1 - 12)
        public decimal Revenue { get; set; } // Doanh thu tháng đó
    }

    public class YearlyRevenueViewModel
    {
        public int Year { get; set; }
        public decimal Revenue { get; set; }
    }

}
