using Telegram_back.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Telegram_back.Service
{
    public class RegisterService
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<RegisterService> _logger;
        private readonly ITokenService _tokenService;

        public RegisterService(ApplicationContext context, ILogger<RegisterService> logger, ITokenService tokenService)
        {
            _context = context;
            _logger = logger;
            _tokenService = tokenService;
        }

        public User? FindUser(LoginDto request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            return user;
        }

        public User? Login(LoginDto request)
        {
            foreach (var u in _context.Users)
            {
                Console.WriteLine($"ID: {u.Id}, Email: {u.Email}, Username: {u.Username}");
            }
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null) return null;

            var passwordHasher = new PasswordHasher<User>();
            var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            var token = "";
            if (verificationResult == PasswordVerificationResult.Success)
            {

                token = _tokenService.GenerateJwtToken(user.Email, user.Username);


                user.Token = token;
                _context.SaveChanges();
                _context.Entry(user).Reload();

                Console.WriteLine($"Reloaded token: {user.Token}");

                return user;
            }

            return user;
        }

        public User Register(RegisterDto request)
        {
            if (_context.Users.Any(u => u.Username == request.Username))
            {
                throw new Exception("Пользователь с таким именем уже существует.");
            }

            if (_context.Users.Any(u => u.Email == request.Email))
            {
                throw new Exception("Пользователь с таким email уже существует.");
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email
            };

            var passwordHasher = new PasswordHasher<User>();
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

            var token = _tokenService.GenerateJwtToken(user.Email, user.Username);
            user.Token = token;

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public List<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }

        // Добавь метод обновления профиля (пример):
        //public User? UpdateUserProfile(Guid userId, UpdateUserDto updateDto)
        //{
        //    var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        //    if (user == null) return null;

        //    if (!string.IsNullOrWhiteSpace(updateDto.Username))
        //        user.Username = updateDto.Username;

        //    if (!string.IsNullOrWhiteSpace(updateDto.Email))
        //        user.Email = updateDto.Email;

        //    if (!string.IsNullOrEmpty(updateDto.AvatarUrl))
        //        user.AvatarUrl = updateDto.AvatarUrl;

        //    if (!string.IsNullOrWhiteSpace(updateDto.Password))
        //    {
        //        var passwordHasher = new PasswordHasher<User>();
        //        user.PasswordHash = passwordHasher.HashPassword(user, updateDto.Password);
        //    }

        //    _context.SaveChanges();
        //    return user;
        //}
    }
}
