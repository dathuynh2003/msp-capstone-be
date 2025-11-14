using Microsoft.EntityFrameworkCore;
using MSP.Application.Repositories;
using MSP.Domain.Entities;
using MSP.Infrastructure.Persistence.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Infrastructure.Repositories
{
    public class LimitationRepository(ApplicationDbContext context) : GenericRepository<Limitation, Guid>(context), ILimitationRepository
    {
        public async Task<Limitation?> GetLimitationByIdAsync(Guid id)
        {
            return await _context.Limitations
               .FirstOrDefaultAsync(m => m.Id == id);
        }
        public async Task<List<Limitation>> GetAll()
        {
            return await _context.Limitations
                .Where(l => !l.IsDeleted)
                .ToListAsync();
        }
        public async Task<List<Limitation>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            return await _context.Limitations
                .Where(l => ids.Contains(l.Id) && !l.IsDeleted)
                .ToListAsync();
        }

    }
}
