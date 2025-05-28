using MongoDB.Driver;
using Vintellitour_Framework.Models.Entities;

namespace Vintellitour_Framework.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<Province> Provinces => _database.GetCollection<Province>("Provinces");
        public IMongoCollection<Location> Locations => _database.GetCollection<Location>("locations");
    }
}
