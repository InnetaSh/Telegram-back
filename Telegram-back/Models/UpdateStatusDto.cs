namespace Telegram_back.Models
{
    public class UpdateStatusDto
    {
        public int MessageId { get; set; }
        public MessageStatus Status { get; set; }
    }
}
