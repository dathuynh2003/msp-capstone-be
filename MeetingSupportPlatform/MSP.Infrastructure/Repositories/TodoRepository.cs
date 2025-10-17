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
    public class TodoRepository(ApplicationDbContext context) : GenericRepository<Todo, Guid>(context), ITodoRepository
    {
        public async Task<IEnumerable<Todo>> GetTodoByMeetingId(Guid meetingId)
        {
            var todos = await _context.Todos.Where(t => t.MeetingId == meetingId)
                .Include(t => t.User)
                .ToListAsync();
            return todos;

        }
    }
}
