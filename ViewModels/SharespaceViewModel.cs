using System.Collections.Generic;
using Vintellitour_Framework.Models; // Nếu bạn dùng model Post, Province trong đó

namespace Vintellitour_Framework.ViewModels
{
    public class SharespaceViewModel
    {
        public string SelectedProvinceId { get; set; }
        public List<ProvinceViewModel> Provinces { get; set; }
        public List<PostViewModel> Posts { get; set; }
        public bool ShowOnlyMyPosts { get; set; }
        public string CurrentUserId { get; set; }
        public Dictionary<string, List<BadgeViewModel>> AuthorBadges { get; set; }
        public string SortOrder { get; set; } = "newest";
        public int PostsCount { get; set; }
        public int CommentsCount { get; set; }
        public List<BadgeViewModel> CurrentUserBadges { get; set; } = new List<BadgeViewModel>();
        public Dictionary<string, List<CommentViewModel>> CommentsForPosts { get; set; }
    }

    public class ProvinceViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class BadgeViewModel
    {
        public string Type { get; set; }
        public string Label { get; set; }
    }
}
