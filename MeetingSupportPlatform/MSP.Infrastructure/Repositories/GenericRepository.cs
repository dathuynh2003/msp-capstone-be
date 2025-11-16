using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MSP.Application.Abstracts;
using MSP.Domain.Base;
using MSP.Infrastructure.Persistence.DBContext;
using MSP.Shared.Common;
using System.Linq.Expressions;

namespace MSP.Infrastructure.Repositories
{
    /// <summary>
    /// GenericRepository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class GenericRepository<T, TKey> : IGenericRepository<T, TKey> where T : class, IBaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync(bool asNoTracking = true)
        {
            IQueryable<T> query = _dbSet;
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true)
        {
            IQueryable<T> query = _dbSet.Where(predicate);
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(TKey id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> AddAsync(T entity)
        {
            _ = await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            return entities;
        }

        public Task UpdateAsync(T entity)
        {
            _ = _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            _ = _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task DeleteByIdAsync(TKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _ = _dbSet.Remove(entity);
            }
        }

        public Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task SaveChangesAsync()
        {
            _ = await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAllWithIncludeAsync(Func<IQueryable<T>, IQueryable<T>>? include = null, bool asNoTracking = true)
        {
            IQueryable<T> query = _dbSet;
            if (asNoTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindWithIncludeAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? include = null, bool asNoTracking = true)
        {
            IQueryable<T> query = _dbSet.Where(predicate);
            if (asNoTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);
            return await query.ToListAsync();
        }

        public void SaveChanges()
        {
            _ = _context.SaveChanges();
        }

        public async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, bool asNoTracking = true)
        {
            IQueryable<T> query = _dbSet;
            if (asNoTracking)
                query = query.AsNoTracking();

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<T>> FindPagedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize, bool asNoTracking = true)
        {
            IQueryable<T> query = _dbSet.Where(predicate);
            if (asNoTracking)
                query = query.AsNoTracking();

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<T>> FindWithIncludePagedAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include,
            int pageNumber,
            int pageSize,
            bool asNoTracking = true)
        {
            IQueryable<T> query = _dbSet.Where(predicate);
            if (asNoTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public Task SoftDeleteAsync(T entity)
        {
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _ = _context.Set<T>().Attach(entity);
            }

            entity.IsDeleted = true;
            return Task.CompletedTask;
        }

        public async Task SoftDeleteByIdAsync(TKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await SoftDeleteAsync(entity);
            }
        }

        public Task HardDeleteAsync(T entity)
        {
            _ = _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task HardDeleteByIdAsync(TKey id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await HardDeleteAsync(entity);
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync(
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool asNoTracking = true)
        {
            IQueryable<T> query = _dbSet;
            if (asNoTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);
            if (orderBy != null)
                query = orderBy(query);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool asNoTracking = true)
        {
            IQueryable<T> query = _dbSet.Where(predicate);
            if (asNoTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);
            if (orderBy != null)
                query = orderBy(query);

            return await query.ToListAsync();
        }

        public async Task<PagingResponse<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool asNoTracking = true)
        {
            IQueryable<T> query = _dbSet;
            if (asNoTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);
            if (orderBy != null)
                query = orderBy(query);

            var totalCount = await query.CountAsync(x => !x.IsDeleted);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResponse<T>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PagingResponse<T>> FindPagedAsync(
            Expression<Func<T, bool>> predicate,
            int pageNumber,
            int pageSize,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool asNoTracking = true)
        {
            IQueryable<T> query = _dbSet.Where(predicate);
            if (asNoTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);
            if (orderBy != null)
                query = orderBy(query);

            var totalCount = await query.CountAsync(x => !x.IsDeleted);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagingResponse<T>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        public async Task<IEnumerable<T>> GetAllWithIncludeAsync(
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool asNoTracking = true)
        {
            IQueryable<T> query = _dbSet;
            if (asNoTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);
            if (orderBy != null)
                query = orderBy(query);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindWithIncludeAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool asNoTracking = true)
        {
            IQueryable<T> query = _dbSet.Where(predicate);
            if (asNoTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);
            if (orderBy != null)
                query = orderBy(query);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool asNoTracking = true)
        {
            IQueryable<T> query = _dbSet;
            if (asNoTracking)
                query = query.AsNoTracking();
            if (orderBy != null)
                query = orderBy(query);
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<T>> FindPagedAsync(
            Expression<Func<T, bool>> predicate,
            int pageNumber,
            int pageSize,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool asNoTracking = true)
        {
            IQueryable<T> query = _dbSet.Where(predicate);
            if (asNoTracking)
                query = query.AsNoTracking();
            if (orderBy != null)
                query = orderBy(query);
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<T>> FindWithIncludePagedAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int pageNumber = 1,
            int pageSize = 10,
            bool asNoTracking = true)
        {
            IQueryable<T> query = _dbSet.Where(predicate);
            if (asNoTracking)
                query = query.AsNoTracking();
            if (include != null)
                query = include(query);
            if (orderBy != null)
                query = orderBy(query);
            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
    }
}
