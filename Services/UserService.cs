using Vintellitour_Framework.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BCrypt.Net;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Vintellitour_Framework.Services
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(string username, string email, string password, HttpRequest request);
        Task<User> LoginUserAsync(string email, string password);
        Task<List<User>> GetUsersByIdsAsync(List<string> userIds);
        Task<User> GetUserIdAsync(string userId);
        Task UpdateUserAsync(User user);
        Task<bool> VerifyUserAsync(string token);
        Task<User> GetUserByEmailAsync(string email);
        // Lấy user theo reset token
        Task<User> GetUserByResetTokenAsync(string token);

        // Đặt lại mật khẩu mới
        Task ResetPasswordAsync(User user, string newPassword);
    }

    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly SmtpSettings _smtpSettings;

        public UserService(MongoDbService mongoDbService, IOptions<SmtpSettings> smtpOptions)
        {
            _users = mongoDbService.GetUserCollection();
            _smtpSettings = smtpOptions.Value;
        }

        public async Task<List<User>> GetUsersByIdsAsync(List<string> userIds)
        {
            var filter = Builders<User>.Filter.In(u => u.Id, userIds);
            return await _users.Find(filter).ToListAsync();
        }

        public async Task<User> GetUserIdAsync(string userId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            return await _users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User> RegisterUserAsync(string username, string email, string password, HttpRequest request)
        {
            var existingUser = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (existingUser != null) return null;

            string verificationToken = Guid.NewGuid().ToString();

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new User
            {
                Username = username,
                Email = email,
                Password = passwordHash,
                IsVerified = false,
                VerificationToken = verificationToken,
                ResetPasswordToken = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await _users.InsertOneAsync(newUser);

            // Gửi mail xác thực với baseUrl lấy từ request
            await SendVerificationEmail(request, newUser.Email, verificationToken, newUser.Username);

            return newUser;
        }

        public async Task<bool> VerifyUserAsync(string token)
        {
            var filter = Builders<User>.Filter.Eq(u => u.VerificationToken, token);
            var user = await _users.Find(filter).FirstOrDefaultAsync();
            if (user == null) return false;

            user.IsVerified = true;
            user.VerificationToken = null; // xóa token sau khi xác thực

            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);

            return true;
        }

        public async Task<User> LoginUserAsync(string email, string password)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null) return null;

            bool verified = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!verified) return null;

            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Id))
                throw new ArgumentException("User hoặc User.Id không hợp lệ");

            user.UpdatedAt = DateTime.UtcNow;

            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            var update = Builders<User>.Update
                .Set(u => u.Username, user.Username)
                .Set(u => u.Avatar, user.Avatar)
                .Set(u => u.ResetPasswordToken, user.ResetPasswordToken)  // Thêm dòng này
                .Set(u => u.UpdatedAt, user.UpdatedAt);

            await _users.UpdateOneAsync(filter, update);
        }


        private async Task SendVerificationEmail(HttpRequest request, string email, string token, string username)
        {
            var baseUrl = $"{request.Scheme}://{request.Host.Value}";
            var verifyUrl = $"{baseUrl}/Account/VerifyEmail?token={token}";

            // Giao diện email theo yêu cầu, dùng username động và verifyUrl động
            string htmlContent = $@"
                <div style=""font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px;"">
                  <div style=""max-width: 600px; margin: auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 4px 10px rgba(0,0,0,0.1);"">
                    <h2 style=""color: #1a73e8;"">Chào {username},</h2>
                    <p style=""font-size: 16px; color: #333;"">
                      Vui lòng nhấn vào nút bên dưới để xác thực tài khoản của bạn:
                    </p>
                    <p style=""text-align: center; margin: 30px 0;"">
                      <a href=""{verifyUrl}"" 
                         style=""background-color: #1a73e8; color: white; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;""
                         target=""_blank""
                         rel=""noopener noreferrer"">
                         Xác thực tài khoản
                      </a>
                    </p>
                    <p style=""font-size: 14px; color: #666;"">Nếu bạn không tạo tài khoản này, vui lòng bỏ qua email này.</p>
                    <p style=""font-size: 14px; color: #666;"">Xin cảm ơn!</p>
                  </div>
                </div>
                ";

            var message = new MailMessage();
            message.From = new MailAddress(_smtpSettings.UserName, "VintelliTour");
            message.To.Add(email);
            message.Subject = "Xác thực tài khoản VintelliTour";
            message.Body = htmlContent;
            message.IsBodyHtml = true;

            using (var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port))
            {
                client.Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password);
                client.EnableSsl = _smtpSettings.EnableSSL;
                await client.SendMailAsync(message);
            }
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            return await _users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByResetTokenAsync(string token)
        {
            var filter = Builders<User>.Filter.Eq(u => u.ResetPasswordToken, token);
            return await _users.Find(filter).FirstOrDefaultAsync();
        }

        public async Task ResetPasswordAsync(User user, string newPassword)
        {
            string newHashed = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.Password = newHashed;
            user.ResetPasswordToken = null;
            // Loại bỏ ResetTokenExpiry khỏi update
            // user.ResetTokenExpiry = null; 

            user.UpdatedAt = DateTime.UtcNow;

            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            var update = Builders<User>.Update
                .Set(u => u.Password, user.Password)
                .Set(u => u.ResetPasswordToken, null)
                // Bỏ dòng cập nhật ResetTokenExpiry
                //.Set(u => u.ResetTokenExpiry, null)
                .Set(u => u.UpdatedAt, user.UpdatedAt);

            await _users.UpdateOneAsync(filter, update);
        }

    }
}
