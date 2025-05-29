namespace Telegram_back.Models
{

    public enum MessageStatus
    {
        Sent = 0,
        Delivered = 1,
        Read = 2
    }

    public class Message
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public string? MediaUrl { get; set; } 
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public MessageStatus Status { get; set; } = MessageStatus.Sent;

        public MediaType MediaType { get; set; }

        public int SenderId { get; set; }
        public User Sender { get; set; }

        public int ChatId { get; set; }
        public Chat Chat { get; set; }
    }
}
