using IdentityServer.Data;
using IdentityServer.Interfaces.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace IdentityServer.Contracts
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);

        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task DeleteAsync(Guid guid)
        {
            var entity = await GetByIdAsync(guid);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
        }


        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            var query = SetIncludes(includes);
            return await query.FirstOrDefaultAsync(predicate);
        }


        public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            var query = SetIncludes(includes);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            var query = SetIncludes(includes);
            return query.Where(predicate);
        }


        private IQueryable<T> SetIncludes(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return query;
        }
    }
}
