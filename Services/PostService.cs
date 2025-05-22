using MongoDB.Driver;
using Vintellitour_Framework.Models;
using Vintellitour_Framework.Services;

public interface IPostService
{
    Task<List<PostViewModel>> GetPostsWithAuthorInfoAsync();
    Task<List<Provinces>> GetProvincesAsync();
    Task UpdatePostLikesAsync(string postId, List<string> usersLiked, int likesCount);
    Task<Post> GetPostByIdAsync(string postId);
    Task<List<Comment>> GetCommentsByPostIdAsync(string postId);
    Task<bool> AddCommentToPostAsync(string postId, Comment comment);
}
public class PostService : IPostService
{
    private readonly IMongoCollection<Post> _posts;
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<Provinces> _provinces;

    public PostService(MongoDbService mongoDbService)
    {
        _posts = mongoDbService.GetPostCollection();
        _users = mongoDbService.GetUserCollection();
        _provinces = mongoDbService.GetProvincesCollection();
    }

    public async Task<List<PostViewModel>> GetPostsWithAuthorInfoAsync()
    {
        var posts = await _posts.Find(_ => true).ToListAsync();

        var authorIds = posts.Select(p => p.AuthorId).Distinct().ToList();

        var filter = Builders<User>.Filter.In(u => u.Id, authorIds);
        var users = await _users.Find(filter).ToListAsync();

        var result = posts.Select(p =>
        {
            var author = users.FirstOrDefault(u => u.Id == p.AuthorId);
            return new PostViewModel
            {
                Post = p,
                AuthorName = author?.Username ?? "Unknown",
                AuthorAvatar = author?.Avatar ?? ""
            };
        }).ToList();

        return result;
    }
    public async Task<Post> GetPostByIdAsync(string postId)
    {
        var filter = Builders<Post>.Filter.Eq(p => p.Id, postId);
        return await _posts.Find(filter).FirstOrDefaultAsync();
    }
    public async Task UpdatePostLikesAsync(string postId, List<string> usersLiked, int likesCount)
    {
        var filter = Builders<Post>.Filter.Eq(p => p.Id, postId);
        var update = Builders<Post>.Update
            .Set(p => p.UsersLiked, usersLiked)
            .Set(p => p.Likes, likesCount);

        await _posts.UpdateOneAsync(filter, update);
    }
    public async Task<List<Comment>> GetCommentsByPostIdAsync(string postId)
    {
        var post = await _posts.Find(p => p.Id == postId).FirstOrDefaultAsync();
        return post?.Comments ?? new List<Comment>();
    }

    public async Task<bool> AddCommentToPostAsync(string postId, Comment comment)
    {
        try
        {
            var filter = Builders<Post>.Filter.Eq(p => p.Id, postId);

            // Khởi tạo Comments array nếu null
            var updateInit = Builders<Post>.Update.SetOnInsert(p => p.Comments, new List<Comment>());
            await _posts.UpdateOneAsync(filter, updateInit, new UpdateOptions { IsUpsert = false });

            // Push comment mới
            var update = Builders<Post>.Update.Push(p => p.Comments, comment);
            var result = await _posts.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            // Log error nếu cần
            Console.WriteLine($"Error adding comment: {ex.Message}");
            return false;
        }
    }
    public async Task<List<Provinces>> GetProvincesAsync()
    {
        return await _provinces.Find(_ => true).ToListAsync();
    }
}

