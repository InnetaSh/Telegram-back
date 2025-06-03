using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using Microsoft.Extensions.Options;
using Telegram_back.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Telegram_back.Controllers
{
    [ApiController]
    [Route("api/speech")]


    public class SpeechController : ControllerBase
    {
        private readonly AzureSpeechSettings _azureSpeechSettings;

        public SpeechController(IOptions<AzureSpeechSettings> azureSpeechOptions)
        {
            _azureSpeechSettings = azureSpeechOptions.Value;
        }

        [HttpPost("recognize")]
        public async Task<IActionResult> RecognizeSpeech(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
                return BadRequest(new { error = "Файл не получен" });

            var tempFilePath = Path.GetTempFileName();

            try
            {

                using (var stream = System.IO.File.Create(tempFilePath))
                {
                    await audioFile.CopyToAsync(stream);
                }


                if (!System.IO.File.Exists(tempFilePath))
                    return BadRequest(new { error = "Временный файл не найден." });

                var fileInfo = new FileInfo(tempFilePath);
                if (fileInfo.Length == 0)
                    return BadRequest(new { error = "Временный файл пуст." });

                var speechConfig = SpeechConfig.FromSubscription(_azureSpeechSettings.AzureSpeechKey, _azureSpeechSettings.AzureSpeechRegion);
                speechConfig.SpeechRecognitionLanguage = "ru-RU";

                using var audioInput = AudioConfig.FromWavFileInput(tempFilePath);
                using var recognizer = new SpeechRecognizer(speechConfig, audioInput);

                var result = await recognizer.RecognizeOnceAsync();

                if (result.Reason == ResultReason.RecognizedSpeech)
                    return Ok(new { text = result.Text });
                else
                    return BadRequest(new { error = "Речь не распознана" });
            }
            finally
            {
                try
                {
                    if (System.IO.File.Exists(tempFilePath))
                        System.IO.File.Delete(tempFilePath);
                }
                catch { }
            }
        }

        [HttpPost("SpeakText")]
        public async Task<IActionResult> SpeakTextAsync([FromBody] TextRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest(new { error = "Text не распознан" });

            var config = SpeechConfig.FromSubscription(_azureSpeechSettings.AzureSpeechKey, _azureSpeechSettings.AzureSpeechRegion);
            config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm);

            var tempFilePath = Path.GetTempFileName();

            try
            {
                using (var fileOutput = AudioConfig.FromWavFileOutput(tempFilePath))
                using (var synthesizer = new SpeechSynthesizer(config, fileOutput))
                {
                    var result = await synthesizer.SpeakTextAsync(request.Text);

                    if (result.Reason == ResultReason.Canceled)
                    {
                        var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                        return BadRequest(new
                        {
                            error = "Ошибка синтеза",
                            details = cancellation.ErrorDetails
                        });
                    }
                }

         
                var audioBytes = await System.IO.File.ReadAllBytesAsync(tempFilePath);
                return File(audioBytes, "audio/wav", "speech.wav");
            }
            finally
            {
                if (System.IO.File.Exists(tempFilePath))
                {
                    try { System.IO.File.Delete(tempFilePath); } catch { }
                }
            }
        }
    }

    }
