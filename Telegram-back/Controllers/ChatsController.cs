using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Telegram_back.Models;

[ApiController]
[Route("api/[controller]")]
public class ChatsController : ControllerBase
{
    private readonly ApplicationContext _context;

    public ChatsController(ApplicationContext context)
    {
        _context = context;
    }

    /// Поиск пользователей по имени.
    //[HttpGet("private-contacts")]
    //public async Task<IActionResult> GetPrivateChatContacts([FromQuery] int userId)
    //{
    //    var contacts = await _context.ChatUsers
    //        .Where(cu => cu.UserId == userId && !cu.Chat.IsGroup)
    //        .SelectMany(cu => cu.Chat.ChatUsers)
    //        .Where(cu => cu.UserId != userId) 
    //        .Select(cu => new
    //        {
    //            cu.User.Id,
    //            cu.User.Username,
    //            cu.User.Email,
    //            cu.User.AvatarUrl,
    //            ChatId = cu.ChatId
    //        })
    //        .Distinct() 
    //        .ToListAsync();

    //    return Ok(contacts);
    //}
    [HttpGet("all-contacts")]
    public async Task<IActionResult> GetAllChatContacts([FromQuery] int userId)
    {
     
        var chats = await _context.Chats
            .Where(c => c.ChatUsers.Any(cu => cu.UserId == userId))
            .Include(c => c.ChatUsers)
                .ThenInclude(cu => cu.User)
            .ToListAsync();

        var result = chats.Select(chat => new
        {
            ChatId = chat.Id,
            IsGroup = chat.IsGroup,
            Title = chat.IsGroup ? chat.Title : chat.ChatUsers.FirstOrDefault(u => u.UserId != userId)?.User.Username,
            AvatarUrl = chat.IsGroup
                ? "https://cdn-icons-png.flaticon.com/512/166/166258.png"
                : chat.ChatUsers.FirstOrDefault(u => u.UserId != userId)?.User.AvatarUrl,
            Members = chat.ChatUsers.Select(cu => new
            {
                cu.User.Id,
                cu.User.Username,
                cu.User.AvatarUrl,
                cu.User.Email
            })
        });

        return Ok(result);
    }




    /// Создание нового чата (приватный или групповой).

    [HttpPost("create")]
    public async Task<IActionResult> CreateChat([FromBody] CreateChatDto dto)
    {
        var users = await _context.Users
            .Where(u => dto.UserIds.Contains(u.Id))
            .ToListAsync();

        if (users.Count != dto.UserIds.Count)
            return BadRequest("Один или несколько пользователей не найдены");

        var chat = new Chat
        {
            Title = dto.Title,
            IsGroup = dto.IsGroup,
            CreatedAt = DateTime.UtcNow,
            ChatUsers = users.Select(u => new ChatUser
            {
                UserId = u.Id
            }).ToList()
        };

        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();

        return Ok(new { chat.Id, chat.Title, chat.IsGroup });
    }

    
    /// Добавить пользователя в группу.
  
    [HttpPost("{chatId}/add-user/{userId}")]
    public async Task<IActionResult> AddUserToGroup(int chatId, int userId)
    {
        var chat = await _context.Chats
            .Include(c => c.ChatUsers)
            .FirstOrDefaultAsync(c => c.Id == chatId && c.IsGroup);

        if (chat == null)
            return NotFound("Группа не найдена");

        if (chat.ChatUsers.Any(cu => cu.UserId == userId))
            return BadRequest("Пользователь уже в группе");

        chat.ChatUsers.Add(new ChatUser { ChatId = chatId, UserId = userId });
        await _context.SaveChangesAsync();

        return Ok("Пользователь добавлен");
    }


    /// Удалить чат (и все сообщения в нём).
    
    [HttpDelete("{chatId}")]
    public async Task<IActionResult> DeleteChat(int chatId)
    {
        var chat = await _context.Chats
            .Include(c => c.Messages)
            .Include(c => c.ChatUsers)
            .FirstOrDefaultAsync(c => c.Id == chatId);

        if (chat == null)
            return NotFound("Чат не найден");

        _context.Messages.RemoveRange(chat.Messages);
        _context.ChatUsers.RemoveRange(chat.ChatUsers);
        _context.Chats.Remove(chat);

        await _context.SaveChangesAsync();

        return Ok("Чат удалён");
    }


    /// Поиск пользователей по имени.





    [HttpGet("search-users")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query)
    {
        var users = await _context.Users
            .Where(u => u.Username.Contains(query))
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                AvatarUrl = u.AvatarUrl
            })
            .ToListAsync();

        return Ok(users);
    }
}
