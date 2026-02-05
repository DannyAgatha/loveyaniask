using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PhoenixLib.Logging;

public static class DiscordLogger
{
    private static readonly HttpClient HttpClient = new();
    private static readonly string ErrorWebHookUrl = Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL") ?? "https://discord.com/api/webhooks/1404762695219019807/PFdmOxQZnl60iu21SuxhYjltNZtWJB-KMwAUftzMlQUZltMy5FBxBQkDj7uA8VmmzBYv";
    private static readonly ConcurrentDictionary<string, string> SentMessages = new();
    
    public static async Task SendErrorToDiscordAsync(string info, Exception ex)
    {
        string messageKey = $"{info}|{ex.Message}";
        string stackTrace = ex.StackTrace ?? "None";
        
        if (SentMessages.TryGetValue(messageKey, out string lastStackTrace) && lastStackTrace == stackTrace)
        {
            Log.Debug("Duplicate error detected, ignoring.");
            return;
        }
        
        SentMessages[messageKey] = stackTrace;
        
        if ((info == "[GRACEFUL_SHUTDOWN] SIGINT received" && ex.Message == "[GRACEFUL_SHUTDOWN] SIGINT received") ||
            (info == "[GRACEFUL_SHUTDOWN] SIGTERM received" && ex.Message == "[GRACEFUL_SHUTDOWN] SIGTERM received" && ex.StackTrace == null))
        {
            Log.Debug("Ignoring [GRACEFUL_SHUTDOWN] SIGINT/SIGTERM received error.");
            return;
        }
        
        bool isStackTraceTooLong = stackTrace.Length > 900;
        
        if (isStackTraceTooLong)
        {
            string filePath = await SaveStackTraceToFileAsync(stackTrace);
            await SendFileToDiscordAsync(filePath);
            File.Delete(filePath);
        }
        else
        {
            object embed = CreateEmbed(info, ex.Message, stackTrace);
            
            HttpResponseMessage response = await HttpClient.PostAsJsonAsync(ErrorWebHookUrl, embed);
            
            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Failed to send message to Discord.", new Exception(response.ReasonPhrase));
            }
        }
    }

    private static object CreateEmbed(string info, string message, string stackTrace)
    {
        return new
        {
            embeds = new[]
            {
                new
                {
                    title = "🔴 Discord Logger Error Service",
                    description = "```diff\n- Error Information\n```",
                    color = 16711680,
                    fields = new object[]
                    {
                        new { name = "🔍 Info", value = $"```css\n{info}\n```", inline = true },
                        new { name = "📝 Message", value = $"```yaml\n{message}\n```", inline = true },
                        new { name = "📚 StackTrace", value = $"```{stackTrace}```" }
                    },
                    footer = new
                    {
                        text = $"Error logged at {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                    },
                    thumbnail = new
                    {
                        url = "https://cdn.pixabay.com/photo/2012/04/02/16/12/x-24850_960_720.png"
                    }
                }
            }
        };
    }

    private static async Task<string> SaveStackTraceToFileAsync(string stackTrace)
    {
        string fileName = $"StackTrace-{DateTime.Now:yyyyMMddHHmmssfff}.txt";
        string filePath = Path.Combine(Path.GetTempPath(), fileName);

        const int maxRetries = 3;
        const int delay = 1000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await File.WriteAllTextAsync(filePath, stackTrace);
                return filePath;
            }
            catch (IOException)
            {
                if (i == (maxRetries - 1))
                {
                    Log.Debug($"Failed to write the stack trace to file after {maxRetries} attempts. File path: {filePath}");
                }

                await Task.Delay(delay);
            }
        }

        return filePath;
    }

    private static async Task SendFileToDiscordAsync(string filePath)
    {
        using var content = new MultipartFormDataContent();
        await using FileStream fileStream = File.OpenRead(filePath);
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        content.Add(streamContent, "file", Path.GetFileName(filePath));
    }
}
