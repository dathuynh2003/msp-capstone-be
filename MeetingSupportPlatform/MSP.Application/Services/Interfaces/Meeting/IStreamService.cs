using MSP.Application.Models;

namespace MSP.Application.Services.Interfaces.Meeting
{
    public interface IStreamService
    {
        Task CreateOrUpdateUserAsync(StreamUserRequest user);
        string GenerateUserToken(string userId);
        Task DeleteCallAsync(string callType, string callId, bool hard = true);

        Task<List<TranscriptionItem>> ListTranscriptionsAsync(string type, string id);

    }
}