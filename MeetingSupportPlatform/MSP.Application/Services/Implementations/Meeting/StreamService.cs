using MSP.Application.Models;
using MSP.Application.Services.Interfaces.Meeting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

public class StreamSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ActionUrl { get; set; } = string.Empty;

}

public class StreamUserRequest
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("role")]
    public string Role { get; set; } = "user";

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("image")]
    public string? Image { get; set; }
}

public class StreamService : IStreamService
{
    private readonly HttpClient _httpClient;
    private readonly StreamSettings _settings;

    public StreamService(HttpClient httpClient, IOptions<StreamSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    // Generate Server Token (dùng API Secret để gọi Stream API)
    private string GenerateServerToken()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.ApiSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var header = new JwtHeader(credentials);
        var payload = new JwtPayload
        {
            { "server", true },
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };

        var token = new JwtSecurityToken(header, payload);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Tạo user trên Stream
    public async Task CreateOrUpdateUserAsync(StreamUserRequest user)
    {
        var url = $"{_settings.BaseUrl}/users?api_key={_settings.ApiKey}";
        var token = GenerateServerToken();

        var body = new
        {
            users = new Dictionary<string, StreamUserRequest>
            {
                { user.Id, user }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", token);
        request.Headers.Add("stream-auth-type", "jwt");
        request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    // Generate user token (để frontend login vào Stream)
    public string GenerateUserToken(string userId)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.ApiSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var header = new JwtHeader(credentials);
        var payload = new JwtPayload
        {
            { "user_id", userId },
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
        };

        var token = new JwtSecurityToken(header, payload);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task DeleteCallAsync(string callType, string callId, bool hard = true)
    {
        var url = $"{_settings.ActionUrl}/video/call/{callType}/{callId}/delete?api_key={_settings.ApiKey}";
        var token = GenerateServerToken();

        var body = new { hard };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", token);
        request.Headers.Add("stream-auth-type", "jwt");
        request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<TranscriptionItem>> ListTranscriptionsAsync(string type, string id)
    {
        var url = $"{_settings.ActionUrl}/video/call/{type}/{id}/transcriptions?api_key={_settings.ApiKey}";
        var token = GenerateServerToken();

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("stream-auth-type", "jwt");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // Dùng Newtonsoft.Json để parse
        var jsonString = await response.Content.ReadAsStringAsync();
        dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonString);

        if (json?.transcriptions != null && json.transcriptions.Count > 0)
        {
            string transcriptUrl = json.transcriptions[0].url.ToString();

            // Lấy nội dung file transcription
            var transcriptContent = await _httpClient.GetStringAsync(transcriptUrl);

            // Parse JSONL thành List<TranscriptionItem>
            var lines = transcriptContent
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => JsonConvert.DeserializeObject<TranscriptionItem>(x))
                .Where(x => x != null)
                .ToList()!;

            return lines;
        }

        return new List<TranscriptionItem>();
    }





}