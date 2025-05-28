using MongoDB.Driver;
using Vintellitour_Framework.Models;

namespace Vintellitour_Framework.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _products;

        public ProductService(MongoDbService mongoDbService)
        {
            _products = mongoDbService.GetProductCollection();
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _products.Find(product => true).ToListAsync();
        }

        public async Task<List<Product>> GetAvailableProductsAsync()
        {
            return await _products.Find(p => p.Stock > 0).ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(string id)
        {
            return await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(string category)
        {
            return await _products.Find(p => p.Category == category && p.Stock > 0).ToListAsync();
        }

        public async Task<List<Product>> GetFilteredProductsAsync(string category, int minPrice, int maxPrice)
        {
            var filterBuilder = Builders<Product>.Filter;

            // Lọc theo tồn kho và giá
            var filter = filterBuilder.Gt(p => p.Stock, 0) &
                         filterBuilder.Gte(p => p.Price, minPrice) &
                         filterBuilder.Lte(p => p.Price, maxPrice);

            // Lọc theo danh mục không phân biệt chữ hoa/thường
            if (!string.IsNullOrEmpty(category))
            {
                filter &= filterBuilder.Regex(p => p.Category,
                    new MongoDB.Bson.BsonRegularExpression($"^{category}$", "i"));
            }

            return await _products.Find(filter).ToListAsync();
        }


        // Method mới để search với text
        public async Task<List<Product>> SearchProductsAsync(string searchText, string category = "", decimal maxPrice = 2000000)
        {
            var filterBuilder = Builders<Product>.Filter;
            var filter = filterBuilder.Gt(p => p.Stock, 0) & filterBuilder.Lte(p => p.Price, maxPrice);

            // Thêm điều kiện tìm kiếm text
            if (!string.IsNullOrEmpty(searchText))
            {
                var textFilter = filterBuilder.Or(
                    filterBuilder.Regex(p => p.Name, new MongoDB.Bson.BsonRegularExpression(searchText, "i")),
                    filterBuilder.Regex(p => p.Description, new MongoDB.Bson.BsonRegularExpression(searchText, "i"))
                );
                filter &= textFilter;
            }

            // Thêm điều kiện category
            if (!string.IsNullOrEmpty(category))
            {
                filter &= filterBuilder.Eq(p => p.Category, category);
            }

            return await _products.Find(filter).ToListAsync();
        }

        // Method để lấy sản phẩm nổi bật
        public async Task<List<Product>> GetFeaturedProductsAsync(int limit = 12)
        {
            return await _products.Find(p => p.Stock > 0 && (p.IsBestSeller || p.IsNew))
                                 .SortByDescending(p => p.IsBestSeller)
                                 .ThenByDescending(p => p.IsNew)
                                 .ThenByDescending(p => p.Rating)
                                 .Limit(limit)
                                 .ToListAsync();
        }

        // Method để lấy sản phẩm mới nhất
        public async Task<List<Product>> GetNewestProductsAsync(int limit = 12)
        {
            return await _products.Find(p => p.Stock > 0)
                                 .SortByDescending(p => p.CreatedAt)
                                 .Limit(limit)
                                 .ToListAsync();
        }

        // Method để lấy sản phẩm bán chạy
        public async Task<List<Product>> GetBestSellerProductsAsync(int limit = 12)
        {
            return await _products.Find(p => p.Stock > 0 && p.IsBestSeller)
                                 .SortByDescending(p => p.Rating)
                                 .ThenByDescending(p => p.Reviews)
                                 .Limit(limit)
                                 .ToListAsync();
        }

        public async Task CreateProductAsync(Product product)
        {
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            await _products.InsertOneAsync(product);
        }

        public async Task UpdateProductAsync(string id, Product updated)
        {
            updated.UpdatedAt = DateTime.UtcNow;
            await _products.ReplaceOneAsync(p => p.Id == id, updated);
        }

        public async Task DeleteProductAsync(string id)
        {
            await _products.DeleteOneAsync(p => p.Id == id);
        }

        // Method để cập nhật stock
        public async Task UpdateStockAsync(string id, int newStock)
        {
            var update = Builders<Product>.Update
                .Set(p => p.Stock, newStock)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            await _products.UpdateOneAsync(p => p.Id == id, update);
        }

        // Method để lấy số lượng sản phẩm theo category
        public async Task<Dictionary<string, int>> GetProductCountByCategoryAsync()
        {
            var pipeline = new[]
            {
                new MongoDB.Bson.BsonDocument("$match", new MongoDB.Bson.BsonDocument("stock", new MongoDB.Bson.BsonDocument("$gt", 0))),
                new MongoDB.Bson.BsonDocument("$group", new MongoDB.Bson.BsonDocument
                {
                    { "_id", "$category" },
                    { "count", new MongoDB.Bson.BsonDocument("$sum", 1) }
                })
            };

            var result = await _products.Aggregate<MongoDB.Bson.BsonDocument>(pipeline).ToListAsync();

            return result.ToDictionary(
                doc => doc["_id"].AsString,
                doc => doc["count"].AsInt32
            );
        }
    }
}