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
using Newtonsoft.Json;
using static NAudio.Wave.WaveInterop;
using System.Text;

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
                ChatId = chat.Id,
                MediaType = dto.MediaType
            };

            try
            {
                _context.Messages.Add(message);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex) { }

            var messageNew = new StoredMessageDto
            {
                MessageId = message.Id,
                Text = dto.Text,
                MediaUrl = url,
                SenderId = user.Id,
                ChatId = chat.Id,
                MediaType = dto.MediaType,
                SentAt = message.SentAt


            };

            var blobUrl = await AppendMessageToBlobAsync(messageNew);



            await _hubContext.Clients.Group(dto.ChatId.ToString()).SendAsync("ReceiveMessage", new
            {
                message.Id,
                message.Text,
                message.SentAt,
                SenderUsername = user.Username,
                SenderId = user.Id,
                message.MediaUrl,
                message.MediaType,
                message.ChatId
            });


            var response = new MessageResponseDto
            {
                Id = message.Id,
                Text = message.Text,
                MediaUrl = message.MediaUrl,
                SentAt = message.SentAt,
                SenderUsername = user.Username,
                SenderId = user.Id,
                MediaType = message.MediaType,
                ChatId = chat.Id
            };

            
            return Ok(response);
        }


   
        private async Task<string> AppendMessageToBlobAsync(StoredMessageDto message)
        {
            var messages = await GetMessagesFromBlobAsync(message.ChatId.ToString());

            messages.Add(message);

            var updatedJson = JsonConvert.SerializeObject(messages, Formatting.Indented);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(updatedJson));

            string blobFileName = $"chat_{message.ChatId}.json";
            string blobUrl = await _blobService.UploadStreamAsync(stream, blobFileName);

            return blobUrl;
        }

        private async Task<List<StoredMessageDto>> GetMessagesFromBlobAsync(string chatId)
        {
            string blobFileName = $"chat_{chatId}.json";
            var blobClient = _blobService.GetBlobClient(blobFileName);

            if (!await blobClient.ExistsAsync())
                return new List<StoredMessageDto>();

            var downloadInfo = await blobClient.DownloadAsync();
            using var reader = new StreamReader(downloadInfo.Value.Content);
            var json = await reader.ReadToEndAsync();
            try
            {
                return JsonConvert.DeserializeObject<List<StoredMessageDto>>(json) ?? new List<StoredMessageDto>();
            }
            catch (Exception ex) {
                return new List<StoredMessageDto>();
            }
        }



        [HttpGet("{chatId}")]
        public async Task<IActionResult> GetMessages(int chatId)
        {

            var messages = await GetMessagesFromBlobAsync(chatId.ToString());
            return Ok(messages);


            //var chat = await _context.Chats
            //    .Include(c => c.Messages)
            //    .ThenInclude(m => m.Sender)
            //    .FirstOrDefaultAsync(c => c.Id == chatId);

            //if (chat == null)
            //    return NotFound("Chat not found");

            //var messages = chat.Messages
            //    .OrderBy(m => m.SentAt)
            //    .Select(m => new MessageDto
            //    {
            //        Id = m.Id,
            //        Text = m.Text,
            //        MediaUrl = m.MediaUrl,
            //        SentAt = m.SentAt,
            //        SenderUsername = m.Sender.Username,
            //        SenderId = m.Sender.Id,
            //        MediaType = m.MediaType
            //    })
            //    .ToList();

            //return Ok(messages);
        }

        [HttpDelete("{chatId}/{msgId}")]
        public async Task<IActionResult> DeleteMessage(int chatId, int msgId)
        {
            var messages = await GetMessagesFromBlobAsync(chatId.ToString());
            var msg =  messages.FirstOrDefault(c => c.MessageId == msgId);

            if (msg.MediaUrl != null && msg.MediaUrl.Length > 0)
            {
                await _blobService.DeleteFileAsync(msg.MediaUrl);

            }

            messages.Remove(msg);

            var updatedJson = JsonConvert.SerializeObject(messages, Formatting.Indented);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(updatedJson));

            string blobFileName = $"chat_{chatId}.json";
            string blobUrl = await _blobService.UploadStreamAsync(stream, blobFileName);



            //var msg = await _context.Messages
            //    .FirstOrDefaultAsync(c => c.Id == msgId);

            //if (msg == null)
            //    return NotFound("Message not found");


            //if (msg.MediaUrl != null && msg.MediaUrl.Length > 0)
            //{
            //    await _blobService.DeleteFileAsync(msg.MediaUrl);

            //}
            //_context.Messages.Remove(msg);

            //await _context.SaveChangesAsync();

            return Ok("Сообщение удалено");
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
