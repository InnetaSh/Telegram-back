using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech.Transcription;
using Microsoft.EntityFrameworkCore;
using Telegram_back.Models; 

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationContext _context;

    public UsersController(ApplicationContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить список контактов пользователя (только приватные чаты).
    /// </summary>
    [HttpGet("{userId}/contacts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContacts(int userId)
    {
        var contacts = await _context.ChatUsers
            .Where(cu => cu.UserId == userId)
            .SelectMany(cu => cu.Chat.ChatUsers)
            .Where(cu => cu.UserId != userId && !cu.Chat.IsGroup)
            .Select(cu => new UserDto
            {
                Id = cu.User.Id,
                Username = cu.User.Username,
                AvatarUrl = cu.User.AvatarUrl
            })
            .Distinct()
            .ToListAsync();

        return Ok(contacts);
    }

    /// <summary>
    /// Получить список групп пользователя.
    /// </summary>
    [HttpGet("{userId}/groups")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroups(int userId)
    {
        var groups = await _context.ChatUsers
            .Where(cu => cu.UserId == userId && cu.Chat.IsGroup)
            .Select(cu => new
            {
                cu.Chat.Id,
                cu.Chat.Title,
                cu.Chat.CreatedAt
            })
            .ToListAsync();

        return Ok(groups);
    }

    /// <summary>
    /// Получить все чаты пользователя (и группы, и приватные).
    /// </summary>
    [HttpGet("{userId}/chats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllChats(int userId)
    {
        var chats = await _context.ChatUsers
            .Where(cu => cu.UserId == userId)
            .Select(cu => new
            {
                cu.Chat.Id,
                cu.Chat.Title,
                cu.Chat.IsGroup,
                cu.Chat.CreatedAt
            })
            .ToListAsync();

        return Ok(chats);
    }

    [HttpGet("by-username")]
    public async Task<IActionResult> GetUserByName([FromQuery] string username)
    {
        var user = await _context.Users
            .Where(u => u.Username == username)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        return Ok(user);
    }
}

