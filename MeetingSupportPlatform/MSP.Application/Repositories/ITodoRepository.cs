using MSP.Application.Abstracts;
using MSP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSP.Application.Repositories
{
    public interface ITodoRepository : IGenericRepository<Todo, Guid>
    {
        Task<IEnumerable<Todo>> GetTodoByMeetingId(Guid meetingId);
    }
}
