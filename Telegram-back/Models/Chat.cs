namespace Telegram_back.Models
{
    public class Chat
    {
        public int Id { get; set; } 
        public bool IsGroup { get; set; } = false;
        public string? Title { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChatUser> ChatUsers { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
