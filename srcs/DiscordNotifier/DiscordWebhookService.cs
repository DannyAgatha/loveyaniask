using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DiscordNotifier;

public class DiscordWebhookService(HttpClient httpClient)
{
    public async Task SendWebhookMessageAsync(string webhookUrl, List<Embed> embeds)
    {
        var payload = new
        {
            embeds = embeds
        };

        string serializedPayload = JsonConvert.SerializeObject(payload);
        var requestContent = new StringContent(serializedPayload, Encoding.UTF8, "application/json");

        await httpClient.PostAsync(webhookUrl, requestContent);
    }

    public class Embed
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("color")]
        public int Color { get; set; }

        [JsonProperty("footer")]
        public Footer Footer { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
        
        [JsonProperty("image")]
        public Image Image { get; set; }

        [JsonProperty("thumbnail")]
        public Thumbnail Thumbnail { get; set; }
    }
    public class Image
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Thumbnail
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
    
    public class Footer
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        
        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }
    }
}