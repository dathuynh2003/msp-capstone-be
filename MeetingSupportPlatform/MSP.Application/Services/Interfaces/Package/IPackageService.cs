using MSP.Application.Models.Requests.Package;
using MSP.Application.Models.Responses.Package;
using MSP.Shared.Common;

namespace MSP.Application.Services.Interfaces.Package
{
    public interface IPackageService
    {
        Task<ApiResponse<List<GetPackageResponse>>> GetAllAsync();
        Task<ApiResponse<GetPackageResponse>> GetByIdAsync(Guid id);
        Task<ApiResponse<GetPackageResponse>> CreateAsync(CreatePackageRequest request);
        Task<ApiResponse<GetPackageResponse>> UpdateAsync(Guid id, UpdatePackageRequest request);
        Task<ApiResponse<string>> DeleteAsync(Guid id);
    }
}
