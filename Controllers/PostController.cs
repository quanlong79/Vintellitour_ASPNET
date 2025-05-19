    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Vintellitour_Framework.ViewModels;  // namespace chứa SharespaceViewModel
    using Vintellitour_Framework.Services;    // IUserService giả sử có lấy badges, provinces

    public class PostsController : Controller
    {
        private readonly IPostService _postService;
        private readonly IUserService _userService;

        public PostsController(IPostService postService, IUserService userService)
        {
            _postService = postService;
            _userService = userService;
        }

        [HttpGet("main/sharespace")]
        public async Task<IActionResult> Sharespace(string selectedProvinceId = null, bool showOnlyMyPosts = false, string sortOrder = "newest")
        {
            var currentUserId = HttpContext.Session.GetString("UserId");

            var provincesFromDb = await _postService.GetProvincesAsync();
            var posts = await _postService.GetPostsWithAuthorInfoAsync();

            if (!string.IsNullOrEmpty(selectedProvinceId))
            {
                posts = posts.FindAll(p => p.Post.ProvinceGid.ToString() == selectedProvinceId);
            }

            if (showOnlyMyPosts && !string.IsNullOrEmpty(currentUserId))
            {
                posts = posts.FindAll(p => p.Post.AuthorId == currentUserId);
            }
            posts = sortOrder switch
            {
                "oldest" => posts.OrderBy(p => p.Post.Timestamp).ToList(),
                _ => posts.OrderByDescending(p => p.Post.Timestamp).ToList(),  // Mặc định newest
            };
            
        // Badges 
            var authorStats = posts
            .GroupBy(p => p.Post.AuthorId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    PostsCount = g.Count(),
                    CommentsCount = g.Sum(p => p.Post.Comments?.Count ?? 0)
                }
            );

        
        var provinces = provincesFromDb.Select(p => new ProvinceViewModel
            {
                Id = p.Id.ToString(),
                Name = p.Name
            }).ToList();
        var badgesList = new Dictionary<string, List<BadgeViewModel>>();
        foreach (var authorId in authorStats.Keys)
        {
            var stats = authorStats[authorId];
            List<BadgeViewModel> badges = new();

            if (stats.PostsCount >= 5 && stats.CommentsCount > 15)
                badges.Add(new BadgeViewModel { Type = "expert", Label = "Chuyên Gia Tương Tác" });
            else if (stats.PostsCount >= 5)
                badges.Add(new BadgeViewModel { Type = "creator", Label = "Người Sáng Tạo" });
            else if (stats.CommentsCount > 15)
                badges.Add(new BadgeViewModel { Type = "connector", Label = "Người Kết Nối" });
            else
                badges.Add(new BadgeViewModel { Type = "rookie", Label = "Tân Binh" });

            badgesList[authorId] = badges;
        }
        
        var viewModel = new SharespaceViewModel
            {
                SelectedProvinceId = selectedProvinceId,
                Provinces = provinces,
                Posts = posts,
                ShowOnlyMyPosts = showOnlyMyPosts,
                CurrentUserId = currentUserId,
                SortOrder = sortOrder,
                AuthorBadges = badgesList
        };
        
        return View("~/Views/Main/Sharespace.cshtml",viewModel);
        }

}
