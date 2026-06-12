// File: MediScope.Data/Repositories/Repository.cs

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Pagination;
namespace MediScope.Data.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }


        public async Task<T?> GetByIdAsync(Guid id)
            => await _dbSet.FindAsync(id);

        public async Task<T?> GetFirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
                query = query.Include(include);

            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
            => await _dbSet
            .Where(e => !(e as BaseEntity)!.IsDeleted)
            .ToListAsync();

        public async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
                query = query.Include(include);

            return await query.Where(predicate).ToListAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.AnyAsync(predicate);


        public async Task AddAsync(T entity)
            => await _dbSet.AddAsync(entity);

        public void Update(T entity)
            => _dbSet.Update(entity);

        public void Remove(T entity)
            => _dbSet.Remove(entity);

        public void SoftDelete(T entity)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.IsDeleted = true;
                baseEntity.DeletedAt = DateTime.UtcNow;
                _dbSet.Update(entity);
            }
            else
            {
                _dbSet.Remove(entity);
            }
        }

        public async Task<PagedResult<T>> GetPagedAsync(
            PaginationParams pagination,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includes)
        {
            // 1. Start with base queryable — EF Core builds SQL lazily
            IQueryable<T> query = _dbSet;

            // 2. Apply includes (JOINs) before filter for EF query plan
            foreach (var include in includes)
                query = query.Include(include);

            // 3. Filter soft-deleted records
            if (typeof(BaseEntity).IsAssignableFrom(typeof(T)))
                query = query.Where(e => !(e as BaseEntity)!.IsDeleted);

            // 4. Apply caller's predicate (e.g. patientId == x)
            if (predicate is not null)
                query = query.Where(predicate);

            // 5. COUNT before pagination — same filter, no Skip/Take
            //    EF translates to: SELECT COUNT(*) FROM ... WHERE ...
            var totalCount = await query.CountAsync();

            // 6. Apply ordering — MUST happen before Skip/Take
            //    Without ordering, SQL Server can return different rows per page
            if (orderBy is not null)
                query = orderBy(query);
            else
                // Default: newest first — cast to BaseEntity for CreatedAt
                query = query.OrderByDescending(
                    e => (e as BaseEntity)!.CreatedAt);

            // 7. Pagination — EF translates to SQL OFFSET/FETCH
            //    .Skip((page-1) * pageSize).Take(pageSize)
            //    e.g. page=2, size=7 → OFFSET 7 ROWS FETCH NEXT 7 ROWS ONLY
            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();   // ← only here does SQL execute

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
            };
        }
    }
}