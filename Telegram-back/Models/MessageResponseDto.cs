namespace Telegram_back.Models
{
    public class MessageResponseDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string MediaUrl { get; set; }
        public DateTime SentAt { get; set; }
        public string SenderUsername { get; set; }
        public int SenderId { get; set; }
    }
}
