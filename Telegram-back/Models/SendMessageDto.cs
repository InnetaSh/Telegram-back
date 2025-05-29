namespace Telegram_back.Models
{

    public enum MediaType
    {
        Image = 0,
        Audio = 1
    }

    public class SendMessageDto
    {
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string? Text { get; set; }
        public IFormFile? MediaUrl { get; set; }
        public MediaType MediaType { get; set; }
    }
}
