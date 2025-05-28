using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Telegram_back.Models;
using Telegram_back.Service;

namespace workUa.Controllers
{
    
    [Route("api/protected")]
    [ApiController]
    public class ProtectedController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly ITokenService _tokenService;
        private readonly RegisterService _regService;


        public ProtectedController(ApplicationContext context, ITokenService tokenService, RegisterService regService)
        {
            _context = context;
            _tokenService = tokenService;
            _regService = regService;
        }


        [HttpGet]
        [Authorize]
        public IActionResult GetSecretData()
        {

            var authorizationHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { message = "Token is missing or invalid." });
            }

            
            var token = authorizationHeader.Substring(7);

            foreach (var u in _context.Users)
            {
                Console.WriteLine($"Email: {u.Email}, Username: {u.Token}");
            }
            Console.WriteLine(token);
            Console.WriteLine($"Token from header: '{token}', length: {token.Length}");
            foreach (var u in _context.Users)
            {
                Console.WriteLine($"User token: '{u.Token}', length: {u.Token.Length}");
            }

            var user = _regService.GetAllUsers().FirstOrDefault(u => u.Token == token);


            if (user != null)
            {
                return Ok(new
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    AvatarUrl = user.AvatarUrl
                });
            }

            return Ok(new { message = "Данных о пользователе нет" });
        }


    }
}
