using MSP.Application.Models.Requests.Meeting;
using MSP.Application.Models.Responses.Meeting;

namespace MSP.Application.Services.Interfaces.Meeting
{
    public interface IStreamService
    {
        Task CreateOrUpdateUserAsync(StreamUserRequest user);
        string GenerateUserToken(string userId);
        Task DeleteCallAsync(string callType, string callId, bool hard = true);

        Task<List<TranscriptionLine>> ListTranscriptionsAsync(string type, string id);

    }
}