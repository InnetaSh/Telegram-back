using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CognitiveServices.Speech;
using System;
using System.IO;
using System.Threading.Tasks;
using Telegram_back.Models;
using Telegram_back.Service;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.CognitiveServices.Speech.Transcription;

namespace Telegram_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly BlobService _blobService;


        private readonly string _speechKey = "<Твой_Speech_Key>";
        private readonly string _speechRegion = "<Твой_Регион>";

        public MessagesController(ApplicationContext context, IHubContext<ChatHub> hubContext, BlobService blobService)
        {
            _context = context;
            _hubContext = hubContext;
            _blobService = blobService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SendMessage([FromForm] SendMessageDto dto)
        {
            var chat = await _context.Chats.FindAsync(dto.ChatId);
            var user = await _context.Users.FindAsync(dto.SenderId);
            if (chat == null || user == null) return NotFound();

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
                ChatId = chat.Id
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(dto.ChatId.ToString()).SendAsync("ReceiveMessage", new
            {
                message.Id,
                message.Text,
                message.SentAt,
                Sender = user.Username
            });

            var response = new MessageResponseDto
            {
                Id = message.Id,
                Text = message.Text,
                MediaUrl = message.MediaUrl,
                SentAt = message.SentAt,
                SenderUsername = user.Username,
                SenderId = user.Id,
            };

            return Ok(response);
        }


        //[HttpPost]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> SendMessage([FromForm] SendMessageDto dto)
        //{
        //    var email = User.FindFirst(ClaimTypes.Email)?.Value;
        //    if (string.IsNullOrEmpty(email)) return Unauthorized();

        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        //    if (user == null) return Unauthorized();

        //    var chat = await _context.Chats.FindAsync(dto.ChatId);
        //    if (chat == null) return NotFound("Чат не найден");

        //    string url = null;
        //    if (dto.MediaUrl != null && dto.MediaUrl.Length > 0)
        //    {
        //        url = await _blobService.UploadFileAsync(dto.MediaUrl);
        //    }

        //    var message = new Message
        //    {
        //        Text = dto.Text,
        //        MediaUrl = url,
        //        SentAt = DateTime.UtcNow,
        //        SenderId = user.Id,
        //        ChatId = chat.Id
        //    };

        //    _context.Messages.Add(message);
        //    await _context.SaveChangesAsync();

        //    await _hubContext.Clients.Group(dto.ChatId.ToString()).SendAsync("ReceiveMessage", new
        //    {
        //        message.Id,
        //        message.Text,
        //        message.SentAt,
        //        Sender = user.Username
        //    });

        //    var response = new MessageResponseDto
        //    {
        //        Id = message.Id,
        //        Text = message.Text,
        //        MediaUrl = message.MediaUrl,
        //        SentAt = message.SentAt,
        //        SenderUsername = user.Username
        //    };

        //    return Ok(response);
        //}







        //[HttpPost]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> SendMessage([FromForm] SendMessageDto dto)
        //{
        //    var chat = await _context.Chats.FindAsync(dto.ChatId);
        //    var user = await _context.Users.FindAsync(dto.SenderId);
        //    if (chat == null || user == null) return NotFound();

        //    string audioUrl = null;

        //    if (!string.IsNullOrWhiteSpace(dto.Text))
        //    {
        //        var config = SpeechConfig.FromSubscription(_speechKey, _speechRegion);
        //        // Можно выбрать формат mp3:
        //        // config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz128KBitRateMonoMp3);

        //        using var synthesizer = new SpeechSynthesizer(config, null);
        //        var result = await synthesizer.SpeakTextAsync(dto.Text);

        //        if (result.Reason == ResultReason.SynthesizingAudioCompleted)
        //        {
        //            using var audioStream = AudioDataStream.FromResult(result);

        //            // Сохраняем аудио во временный файл
        //            var tempFile = Path.GetTempFileName() + ".wav";
        //            await audioStream.SaveToWaveFileAsync(tempFile);

        //            IFormFile formFile = CreateFormFileFromPath(tempFile);

        //            audioUrl = await _blobService.UploadFileAsync(formFile);

        //            // Удаляем временный файл
        //            System.IO.File.Delete(tempFile);
        //        }
        //        else if (result.Reason == ResultReason.Canceled)
        //        {
        //            var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
        //            // Можно залогировать ошибку, но пока пропускаем
        //        }
        //    }

        //    var message = new Message
        //    {
        //        Text = dto.Text,
        //        MediaUrl = audioUrl,   // сюда пишем ссылку на голосовое сообщение
        //        SentAt = DateTime.UtcNow,
        //        SenderId = user.Id,
        //        ChatId = chat.Id
        //    };

        //    _context.Messages.Add(message);
        //    await _context.SaveChangesAsync();

        //    await _hubContext.Clients.Group(dto.ChatId.ToString()).SendAsync("ReceiveMessage", new
        //    {
        //        message.Id,
        //        message.Text,
        //        message.MediaUrl,
        //        message.SentAt,
        //        Sender = user.Username
        //    });

        //    return Ok(message);
        //}



        [HttpGet("{chatId}")]
        public async Task<IActionResult> GetMessages(int chatId)
        {
            var chat = await _context.Chats
                .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
                return NotFound("Chat not found");

            var messages = chat.Messages
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    Text = m.Text,
                    MediaUrl = m.MediaUrl,
                    SentAt = m.SentAt,
                    SenderUsername = m.Sender.Username,
                    SenderId = m.Sender.Id
                })
                .ToList();

            return Ok(messages);
        }

        [HttpDelete("{msgId}")]
        public async Task<IActionResult> DeleteMessage(int msgId)
        {
            var msg = await _context.Messages
                .FirstOrDefaultAsync(c => c.Id == msgId);

            if (msg == null)
                return NotFound("Message not found");

          
            if (msg.MediaUrl != null && msg.MediaUrl.Length > 0)
            {
                await _blobService.DeleteFileAsync(msg.MediaUrl);

            }
            _context.Messages.Remove(msg);

            await _context.SaveChangesAsync();

            return Ok("Сщщбщение удалено");
        }



        [HttpPost("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusDto dto)
        {
            var message = await _context.Messages.FindAsync(dto.MessageId);
            if (message == null)
                return NotFound("Сообщение не найдено");

            if (dto.Status < message.Status)
                return BadRequest("Нельзя откатить статус назад");

            message.Status = dto.Status;
            await _context.SaveChangesAsync();

          
            await _hubContext.Clients.User(message.SenderId.ToString())
                .SendAsync("MessageStatusUpdated", new
                {
                    MessageId = message.Id,
                    Status = message.Status.ToString()
                });

            return Ok();
        }


        private IFormFile CreateFormFileFromPath(string path)
        {
            var stream = System.IO.File.OpenRead(path);
            var fileName = System.IO.Path.GetFileName(path);
            return new FormFile(stream, 0, stream.Length, "file", fileName);
        }
    }

}
