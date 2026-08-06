using System.Text.Json.Serialization;

namespace ServiceHub_IT.Models;

public class Notification
{
    [JsonPropertyName("id")]
    public Guid id { get; set; }

    [JsonPropertyName("user_id")]
    public Guid user_id { get; set; }

    [JsonPropertyName("title")]
    public string title { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string message { get; set; } = string.Empty;

    [JsonPropertyName("is_read")]
    public bool is_read { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? created_at { get; set; } = DateTime.UtcNow;
}
