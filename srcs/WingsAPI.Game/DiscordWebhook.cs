using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public static class DiscordWebhook
{
    private static readonly HttpClient client = new HttpClient();

    public static async Task SendWebhookAsync(string webhookUrl, string title, string description, string imageUrl)
    {
        var payload = new
        {
            embeds = new[]
            {
                new
                {
                    title = title,
                    description = description,
                    image = new { url = imageUrl }
                }
            }
        };

        string jsonPayload = JsonSerializer.Serialize(payload);

        HttpResponseMessage response = await client.PostAsync(webhookUrl, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();
    }
}