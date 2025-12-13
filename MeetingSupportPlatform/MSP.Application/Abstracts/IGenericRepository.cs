using Microsoft.EntityFrameworkCore.Storage;
using MSP.Shared.Common;
using System.Linq.Expressions;

namespace MSP.Application.Abstracts
{
    /// <summary>
    /// IGenericRepository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public interface IGenericRepository<T, TKey> where T : class
    {
        // Basic Get
        Task<IEnumerable<T>> GetAllAsync(
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool asNoTracking = true);

        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool asNoTracking = true);

        // Paging
        Task<PagingResponse<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool asNoTracking = true);

        Task<PagingResponse<T>> FindPagedAsync(
            Expression<Func<T, bool>> predicate,
            int pageNumber,
            int pageSize,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool asNoTracking = true);

        // Shortcuts
        Task<T?> GetByIdAsync(TKey id);
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task UpdateRangeAsync(IEnumerable<T> entities);
        Task SoftDeleteAsync(T entity);
        Task SoftDeleteByIdAsync(TKey id);
        Task HardDeleteAsync(T entity);
        Task HardDeleteByIdAsync(TKey id);
        Task DeleteRangeAsync(IEnumerable<T> entities);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        void SaveChanges();
        Task SaveChangesAsync();

        // Include + Paging + OrderBy
        Task<IEnumerable<T>> GetAllWithIncludeAsync(
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool asNoTracking = true);

        Task<IEnumerable<T>> FindWithIncludeAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool asNoTracking = true);

        Task<IEnumerable<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool asNoTracking = true);

        Task<IEnumerable<T>> FindPagedAsync(
            Expression<Func<T, bool>> predicate,
            int pageNumber,
            int pageSize,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            bool asNoTracking = true);

        Task<IEnumerable<T>> FindWithIncludePagedAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int pageNumber = 1,
            int pageSize = 10,
            bool asNoTracking = true);

        Task<IDbContextTransaction> BeginTransactionAsync();

        IExecutionStrategy CreateExecutionStrategy();

        // Explicitly load navigation property
        Task LoadCollectionAsync<TProperty>(T entity, Expression<Func<T, IEnumerable<TProperty>>> navigationProperty) where TProperty : class;
    }
}
