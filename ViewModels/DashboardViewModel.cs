namespace Vintellitour_Framework.ViewModels
{
    public class DashboardViewModel
    {
        public List<ProvinceEngagementViewModel> ProvinceEngagements { get; set; }
        public UserPostStatusViewModel UserPostStatus { get; set; }
        public List<PostStatsByMonthViewModel> PostStatsByMonth { get; set; }
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
}
