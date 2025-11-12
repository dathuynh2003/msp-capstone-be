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

    }
}
