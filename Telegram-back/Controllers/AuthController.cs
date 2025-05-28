using Azure.Core;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Telegram_back.Models;
using Telegram_back.Service;

namespace Telegram_back.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly ITokenService _tokenService;
        private readonly RegisterService _regService;
        private readonly BlobService _blobService;


        public AuthController(ApplicationContext context, ITokenService tokenService, RegisterService regService, BlobService blobService)
        {
            _blobService = blobService;
            _context = context;
            _tokenService = tokenService;
            _regService = regService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto request)
        {

            if (request == null)
                return BadRequest("Request is null");

            var user = _regService.Login(request);

            if (user == null)
                return Unauthorized("Неверное имя пользователя или пароль");

            //var token = _tokenService.GenerateJwtToken(user.Email, user.Username);

            return Ok(new
            {
                message = "Вход в систему выполнен успешно",
                token = user.Token
            });
        
        }


        [HttpPost("register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Register([FromForm] RegisterDto request)
        {
            if (request == null)
                return BadRequest("Request is null");

            try
            {
                var user = _regService.Register(request);

                if (user == null)
                    return Unauthorized("Неверное имя пользователя или пароль");

                string avatarUrl = null;

                if (request.AvatarUrl != null && request.AvatarUrl.Length > 0)
                {
                   
                    avatarUrl = await _blobService.UploadFileAsync(request.AvatarUrl);

                    
                    user.AvatarUrl = avatarUrl;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }

                var token = _tokenService.GenerateJwtToken(user.Email, user.Username);

                return Ok(new
                {
                    message = "Регистрация и загрузка аватара прошли успешно",
                    token,
                    avatarUrl
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
