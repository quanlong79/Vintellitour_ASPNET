//using MongoDB.Driver;
//using Vintellitour_Framework.Models;
//using Vintellitour_Framework.Services;

//public interface IProvinceService
//{
//    Task<List<Provinces>> GetAllProvincesAsync();
//}

//public class ProvinceService : IProvinceService
//{
//    private readonly IMongoCollection<Provinces> _provinces;

//    public ProvinceService(MongoDbService mongoDbService)
//    {
//        _provinces = mongoDbService.GetProvincesCollection();
//    }

//    public async Task<List<Provinces>> GetAllProvincesAsync()
//    {
//        return await _provinces.Find(_ => true).ToListAsync();
//    }
//}
