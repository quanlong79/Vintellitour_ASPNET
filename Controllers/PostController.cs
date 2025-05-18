using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Vintellitour_Framework.ViewModels;  // namespace chứa SharespaceViewModel
using Vintellitour_Framework.Services;    // IUserService giả sử có lấy badges, provinces

[Route("main")]
public class PostsController : Controller
{
    private readonly IPostService _postService;
    private readonly IUserService _userService;

    public PostsController(IPostService postService, IUserService userService)
    {
        _postService = postService;
        _userService = userService;
    }

    [HttpGet("sharespace")]
    public async Task<IActionResult> Sharespace(string selectedProvinceId = null, bool showOnlyMyPosts = false)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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

        var provinces = provincesFromDb.Select(p => new ProvinceViewModel
        {
            Id = p.Id.ToString(),
            Name = p.Name
        }).ToList();

        var viewModel = new SharespaceViewModel
        {
            SelectedProvinceId = selectedProvinceId,
            Provinces = provinces,
            Posts = posts,
            ShowOnlyMyPosts = showOnlyMyPosts,
            CurrentUserId = currentUserId,
            // AuthorBadges = badges
        };

        return View(viewModel);
    }

}
