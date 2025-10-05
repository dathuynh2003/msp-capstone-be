using MSP.Application.Models;

namespace MSP.Application.Services.Interfaces.Meeting
{
    public interface IWhisperService
    {
        Task<List<TranscriptionItem>> TranscribeVideoAsync(string videoPath);
    }
}
