using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Vintellitour_Framework.Models;
using Vintellitour_Framework.Services;
using Vintellitour_Framework.ViewModels;

public class UserProfileViewComponent : ViewComponent
{
    private readonly IPostService _postService;
    private readonly IUserService _userService;

    public UserProfileViewComponent(IPostService postService, IUserService userService)
    {
        _postService = postService;
        _userService = userService;
    }

    // Phương thức trả view (Invoke hoặc InvokeAsync)
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var currentUserId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(currentUserId))
        {
            // Nếu chưa đăng nhập thì trả về model với badges trống
            var emptyViewModel = new SharespaceViewModel
            {
                AuthorBadges = new Dictionary<string, List<BadgeViewModel>>(),
                CurrentUserBadges = new List<BadgeViewModel>()
            };
            return View(emptyViewModel);
        }
        var posts = await _postService.GetPostsWithAuthorInfoAsync();
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
        var badgesList = new Dictionary<string, List<BadgeViewModel>>();
        var stats = authorStats[currentUserId];
        List<BadgeViewModel> badges = new();


        if (stats.PostsCount >= 5 && stats.CommentsCount > 15)
            badges.Add(new BadgeViewModel { Type = "expert", Label = "Chuyên Gia Tương Tác" });
        else if (stats.PostsCount >= 5)
            badges.Add(new BadgeViewModel { Type = "creator", Label = "Người Sáng Tạo" });
        else if (stats.CommentsCount > 15)
            badges.Add(new BadgeViewModel { Type = "connector", Label = "Người Kết Nối" });
        else
            badges.Add(new BadgeViewModel { Type = "rookie", Label = "Tân Binh" });

        badgesList[currentUserId] = badges;
        List<BadgeViewModel> currentUserBadges = new List<BadgeViewModel>();
        if (!string.IsNullOrEmpty(currentUserId) && badgesList.ContainsKey(currentUserId))
        {
            currentUserBadges = badgesList[currentUserId];
        }

        var viewModel = new SharespaceViewModel
        {
            AuthorBadges = badgesList,
            CurrentUserBadges = currentUserBadges
        };
        return View(viewModel);
    }
}
