using MSP.Application.Abstracts;
using MSP.Application.Models.Requests.Package;
using MSP.Application.Models.Responses.Package;
using MSP.Domain.Entities;
using MSP.Shared.Common;

namespace MSP.Application.Repositories
{
    public interface IPackageRepository: IGenericRepository<Package, Guid>
    {
        Task<Package?> GetPackageByIdAsync(Guid id);
        Task<List<Package>> GetAll();
        Task<List<Package>> GetByIdsAsync(IEnumerable<Guid> ids);
        Task<Package?> GetFreePackageAsync();

    }
}
