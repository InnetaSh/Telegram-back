namespace Telegram_back.Models
{
    public class User
    {
        public int Id { get; set; } 
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } 
        public string? AvatarUrl { get; set; }

        public string Token { get; set; }

        public ICollection<ChatUser> ChatUsers { get; set; }
        public ICollection<Message> Messages { get; set; }
    }

   
    public class UserResponse
    {
        public int Id { get; set; }
        public string Token { get; set; }

    }
  
}
