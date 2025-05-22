    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Vintellitour_Framework.ViewModels;  // namespace chứa SharespaceViewModel
    using Vintellitour_Framework.Services;
    using Vintellitour_Framework.Models;
    using MongoDB.Bson;

// IUserService giả sử có lấy badges, provinces
//\Controllers\PostController.cs
[Route("post")]
public class PostController : Controller
{
    private readonly IPostService _postService;
    private readonly IUserService _userService;
    public PostController(IPostService postService, IUserService userService)
    {
            _postService = postService;
            _userService = userService;
        }

        [HttpGet("sharespace")]
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

        // Gán trạng thái like cho từng post
        if (!string.IsNullOrEmpty(currentUserId))
        {
            posts = posts.Select(p =>
            {
                p.IsLikedByCurrentUser = p.Post.UsersLiked != null && p.Post.UsersLiked.Contains(currentUserId);
                return p;
            }).ToList();
        }
        else
        {
            // Chưa đăng nhập thì mặc định chưa like
            posts = posts.Select(p =>
            {
                p.IsLikedByCurrentUser = false;
                return p;
            }).ToList();
        }



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

        var commentsDict = new Dictionary<string, List<CommentViewModel>>();

        foreach (var post in posts)
        {
            if (post.Post.Comments != null && post.Post.Comments.Any())
            {
                var commentViewModels = new List<CommentViewModel>();

                foreach (var comment in post.Post.Comments)
                {
                    // Lấy thông tin user cho comment
                    var commentUser = await _userService.GetUserIdAsync(comment.UserId);

                    commentViewModels.Add(new CommentViewModel
                    {
                        Id = comment.Id,
                        UserId = comment.UserId,
                        Content = comment.Content,
                        CreatedAt = comment.CreatedAt,
                        Username = commentUser?.Username ?? "Unknown User",
                        Avatar = commentUser?.Avatar ?? "/img/VN.jpg"
                    });
                }

                // Sắp xếp comments theo thời gian mới nhất
                commentsDict[post.Post.Id] = commentViewModels.OrderBy(c => c.CreatedAt).ToList();
            }
            else
            {
                commentsDict[post.Post.Id] = new List<CommentViewModel>();
            }
        }


        var viewModel = new SharespaceViewModel
            {
                SelectedProvinceId = selectedProvinceId,
                Provinces = provinces,
                Posts = posts,
                ShowOnlyMyPosts = showOnlyMyPosts,
                CurrentUserId = currentUserId,
                SortOrder = sortOrder,
                AuthorBadges = badgesList,
                CommentsForPosts = commentsDict

        };
        
        return View(viewModel);
        }

        [HttpPost("togglelike")]
        public async Task<IActionResult> ToggleLike([FromBody] LikeToggleRequest request)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var post = await _postService.GetPostByIdAsync(request.PostId);
            if (post == null) return NotFound();

            bool isLiked;

            if (post.UsersLiked.Contains(userId))
            {
                post.UsersLiked.Remove(userId);
                post.Likes--;
                isLiked = false;
            }
            else
            {
                post.UsersLiked.Add(userId);
                post.Likes++;
                isLiked = true;
            }
            post.UsersLiked ??= new List<string>();
            if (post.Likes < 0) post.Likes = 0;

        // Cập nhật chỉ trường likes và usersLiked
        await _postService.UpdatePostLikesAsync(post.Id, post.UsersLiked, post.Likes);

            return Json(new { success = true, isLiked, likesCount = post.Likes });
    }


    // Thêm vào PostController của bạn:

    [HttpPost("comment")]
    public async Task<IActionResult> PostComment([FromBody] PostCommentRequest request)
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
            return Json(new { success = false, message = "Bạn cần đăng nhập để bình luận" });

        if (string.IsNullOrWhiteSpace(request.Content))
            return Json(new { success = false, message = "Nội dung bình luận không được để trống" });

        if (request.Content.Length > 500)
            return Json(new { success = false, message = "Bình luận không được quá 500 ký tự" });

        try
        {
            // Tạo comment mới
            var newComment = new Comment
            {
                Id = ObjectId.GenerateNewId().ToString(),
                UserId = userId,
                Content = request.Content.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            // Thêm comment vào post
            var success = await _postService.AddCommentToPostAsync(request.PostId, newComment);

            if (!success)
                return Json(new { success = false, message = "Không thể thêm bình luận" });

            // Lấy thông tin user để trả về
            var user = await _userService.GetUserIdAsync(userId);

            var commentViewModel = new CommentViewModel
            {
                Id = newComment.Id,
                UserId = newComment.UserId,
                Content = newComment.Content,
                CreatedAt = newComment.CreatedAt,
                Username = user?.Username ?? "Unknown User",
                Avatar = user?.Avatar ?? "/img/VN.jpg"
            };

            return Json(new
            {
                success = true,
                comment = commentViewModel,
                message = "Bình luận đã được đăng thành công"
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Có lỗi xảy ra khi đăng bình luận" });
        }
    }

    // Thêm class request model:
    public class PostCommentRequest
    {
        public string PostId { get; set; }
        public string Content { get; set; }
    }
}
public class LikeToggleRequest
{
    public string PostId { get; set; }
}
public class CommentViewModel
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Username { get; set; }
    public string Avatar { get; set; }
}
public class PostCommentRequest
{
    public string PostId { get; set; }
    public string Content { get; set; }
}