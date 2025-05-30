using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vintellitour_Framework.Models;

namespace Vintellitour_Framework.Services
{
    public class AdminProductService : IProductService
    {
        private readonly IMongoCollection<Product> _products;

        public AdminProductService(MongoDbService mongoDbService)
        {
            _products = mongoDbService.GetProductCollection();
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _products.Find(_ => true).ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(string id)
        {
            return await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateProductAsync(Product product)
        {
            await _products.InsertOneAsync(product);
        }

        public async Task UpdateProductAsync(Product product)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, product.Id);
            await _products.ReplaceOneAsync(filter, product);
        }

        public async Task DeleteProductAsync(string id)
        {
            await _products.DeleteOneAsync(p => p.Id == id);
        }
    }
}
