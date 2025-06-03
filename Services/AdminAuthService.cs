using MongoDB.Driver;
using System.Threading.Tasks;
using Vintellitour_Framework.Models;

namespace Vintellitour_Framework.Services
{
    public class AdminAuthService
    {
        private readonly IMongoCollection<Admin> _adminsCollection;

        public AdminAuthService(IMongoDatabase database)
        {
            _adminsCollection = database.GetCollection<Admin>("admins");
        }

        // Xác thực email + password
        public async Task<Admin?> AuthenticateAsync(string email, string password)
        {
            var admin = await _adminsCollection.Find(a => a.Email == email && a.Role == "admin").FirstOrDefaultAsync();

            if (admin == null)
                return null;

            // TODO: Ở thực tế, hãy mã hóa và so sánh mật khẩu đã hash
            if (admin.Password != password)
                return null;

            return admin;
        }
    }
}
