namespace Telegram_back.Models
{
    public class StoredMessageDto
    {
        public int MessageId { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string? Text { get; set; }
        public string? MediaUrl { get; set; }
        public MediaType MediaType { get; set; }
    }
}
