using Microsoft.EntityFrameworkCore;
using MSP.Application.Repositories;
using MSP.Domain.Entities;
using MSP.Infrastructure.Persistence.DBContext;

namespace MSP.Infrastructure.Repositories
{
    public class TodoRepository(ApplicationDbContext context) : GenericRepository<Todo, Guid>(context), ITodoRepository
    {
        public async Task<IEnumerable<Todo>> GetTodoByMeetingId(Guid meetingId)
        {
            var todos = await _context.Todos.Where(t => t.MeetingId == meetingId && t.IsDeleted != true &&  t.Status != Shared.Enums.TodoStatus.Deleted)
                .Include(t => t.User)
                .Include(t => t.ReferencedTasks)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
            return todos;

        }

        public async Task<Todo> GetByIdAsync(Guid id)
        {
            var todo = await _context.Todos
                .Where(t => t.Id == id)
                .Include(t => t.Meeting)
                .FirstOrDefaultAsync();
            return todo;
        }

        public async Task<bool> SoftDeleteTodosByMeetingId(Guid meetingId)
        {
            var todos = await _context.Todos
                .Where(t => t.MeetingId == meetingId && t.IsDeleted != true)
                .ToListAsync();
            if (todos.Count == 0)
            {
                return false;
            }
            foreach (var todo in todos)
            {
                todo.IsDeleted = true;
                _context.Todos.Update(todo);
            }
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
