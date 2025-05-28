namespace Telegram_back.Models
{
    public class SendMessageDto
    {
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string? Text { get; set; }
        public IFormFile? MediaUrl { get; set; }
    }
}
