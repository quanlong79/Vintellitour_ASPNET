using MongoDB.Driver;
using Vintellitour_Framework.Models;
using Vintellitour_Framework.Services;

public interface IPostService
{
    Task<List<PostViewModel>> GetPostsWithAuthorInfoAsync();
    Task<List<Provinces>> GetProvincesAsync();
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
    public async Task<List<Provinces>> GetProvincesAsync()
    {
        return await _provinces.Find(_ => true).ToListAsync();
    }
}

