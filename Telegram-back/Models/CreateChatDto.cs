namespace Telegram_back.Models
{
    public class CreateChatDto
    {
        public string? Title { get; set; }
        public bool IsGroup { get; set; }
        public List<int> UserIds { get; set; }
    }
}
