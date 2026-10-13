using Microsoft.EntityFrameworkCore;
using EcommerceBackend.Data;

namespace EcommerceBackend.Repositories
{
    //GenericRepos
    public class GenericRepository<T, TKey> : IGenericRepository<T, TKey> where T : class
    {
        //_appDbContext is 
        private readonly AppDbContext _appDbContext;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _dbSet = appDbContext.Set<T>();
        }
        public async Task<T?> GetByIdAsync(TKey id) => await _dbSet.FindAsync(id);
        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public void Update(T entity) => _dbSet.Update(entity);
        public void Remove(T entity) => _dbSet.Remove(entity);
        public async Task<bool> SaveChangesAsync() => await _appDbContext.SaveChangesAsync()>0;
    }
}
