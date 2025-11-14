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
    public class PackageRepository(ApplicationDbContext context) : GenericRepository<Package, Guid>(context), IPackageRepository
    {
        public async Task<Package?> GetPackageByIdAsync(Guid id)
        {
            return await _context.Packages
               .FirstOrDefaultAsync(m => m.Id == id);
        }
        public async Task<List<Package>> GetAll()
        {
            return await _context.Packages
                .Where(l => !l.IsDeleted)
                .Include(p => p.Limitations)
                .ToListAsync();
        }
        public async Task<List<Package>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            return await _context.Packages
                .Where(p => ids.Contains(p.Id) && !p.IsDeleted)
                .ToListAsync();
        }


    }
}
