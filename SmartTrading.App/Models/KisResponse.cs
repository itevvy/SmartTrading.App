using System.Text.Json.Serialization;

namespace SmartTrading.App.Models;

public class KisTokenResponse
{
    /// <summary>
    /// JSON의 "access_token"을 C#의 AccessToken 프로퍼티로 연결
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}