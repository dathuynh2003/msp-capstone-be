using MSP.Application.Abstracts;
using MSP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Repositories
{
    public interface ILimitationRepository: IGenericRepository<Limitation, Guid>
    {
        Task<Limitation?> GetLimitationByIdAsync(Guid id);
        Task<List<Limitation>> GetAll();
        Task<List<Limitation>> GetByIdsAsync(IEnumerable<Guid> ids);
    }
}
