using System.Collections.Generic;
using System.Threading.Tasks;
using Vintellitour_Framework.Models;

namespace Vintellitour_Framework.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync();

        Task<Product> GetProductByIdAsync(string id);

        Task CreateProductAsync(Product product);

        Task UpdateProductAsync(Product product);

        Task DeleteProductAsync(string id);
    }
}
