using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Telegram_back.Models;

[Authorize] // Требует авторизацию, если используешь JWT
public class ChatHub : Hub
{
    private readonly ApplicationContext _context;

    public ChatHub(ApplicationContext context)
    {
        _context = context;
    }

    // Пользователь присоединяется к группе чата
    public async Task JoinChat(string chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
    }

    // Пользователь выходит из группы чата
    public async Task LeaveChat(string chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
    }

    // Метод отправки сообщения в чат
    public async Task SendMessage(int chatId, string text, string? mediaUrl)
    {
        var userIdStr = Context.UserIdentifier; // или получаем из Claims
        if (!int.TryParse(userIdStr, out var userId))
        {
            // Ошибка: пользователь не аутентифицирован или Id не определён
            throw new HubException("Unauthorized user");
        }

        // Сохраняем сообщение в базе
        var message = new Message
        {
            ChatId = chatId,
            SenderId = userId,
            Text = text,
            MediaUrl = mediaUrl,
            SentAt = DateTime.UtcNow,
            Status = MessageStatus.Sent
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Отправляем сообщение всем участникам группы
        await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", new
        {
            Id = message.Id,
            ChatId = chatId,
            SenderId = userId,
            Text = text,
            MediaUrl = mediaUrl,
            SentAt = message.SentAt,
            Status = message.Status.ToString()
        });
    }
}
