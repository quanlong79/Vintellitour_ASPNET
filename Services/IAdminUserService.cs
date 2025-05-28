using System.Collections.Generic;
using System.Threading.Tasks;
using Vintellitour_Framework.Models;

namespace Vintellitour_Framework.Services
{
    public interface IAdminUserService
    {
        Task<List<AdminUser>> GetAllAsync();
        Task<bool> DeleteAsync(string userId);
        Task<bool> UpdateAsync(string id, string username, string email);
    }
}
