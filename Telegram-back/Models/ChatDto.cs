namespace Telegram_back.Models
{
    public class ChatDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public bool IsGroup { get; set; }
        public List<UserDto> Participants { get; set; }
        public MessageDto? LastMessage { get; set; }
    }
}
