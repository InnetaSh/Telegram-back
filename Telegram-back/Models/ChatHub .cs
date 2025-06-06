using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CognitiveServices.Speech.Transcription;
using System;
using System.Threading.Tasks;
using Telegram_back.Models;
using Telegram_back.Service;

//[Authorize] 
public class ChatHub : Hub
{
    private readonly ApplicationContext _context;
    private readonly BlobService _blobService;

    public ChatHub(ApplicationContext context, BlobService blobService)
    {
        _context = context;
        _blobService = blobService;
    }

    public async Task SendMessage(SendMessageDto dto)
    {
        var chat = await _context.Chats.FindAsync(dto.ChatId);
        var user = await _context.Users.FindAsync(dto.SenderId);
        if (chat == null || user == null) return ;

        string url = null;
        if (dto.MediaUrl != null && dto.MediaUrl.Length > 0)
        {
            url = await _blobService.UploadFileAsync(dto.MediaUrl);

        }
        var message = new Message
        {
            Text = dto.Text,
            MediaUrl = url,
            SentAt = DateTime.UtcNow,
            SenderId = user.Id,
            ChatId = chat.Id,
            MediaType = dto.MediaType
        };


        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // отправляем объект с обновленным Id и временем, как ответ клиентам
        await Clients.Group(dto.ChatId.ToString()).SendAsync("ReceiveMessage", new
        {
            Id = message.Id,
            Text = message.Text,
            MediaUrl = message.MediaUrl,
            SentAt = message.SentAt,
            SenderId = message.SenderId,
            ChatId = message.ChatId,
            MediaType = message.MediaType
        });
    }

    public async Task JoinChat(int chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
    }

    public async Task LeaveChat(int chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
    }
}
