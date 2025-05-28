namespace Telegram_back.Models
{
    public class RegisterDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public IFormFile AvatarUrl { get; set; }
    }
}
