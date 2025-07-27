using Domain.Contracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using Persistence.Data;

public class GenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : BaseEntity
{
    protected readonly FoodCourtDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public GenericRepository(FoodCourtDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
        => await _dbSet.ToListAsync();

    public async Task<TEntity> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public async Task AddAsync(TEntity entity)
        => await _dbSet.AddAsync(entity);

    public void Update(TEntity entity)
        => _dbSet.Update(entity);

    public void Delete(TEntity entity)
        => _dbSet.Remove(entity);

    public async Task<TEntity> GetByIdAsync(int id, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
    {
        IQueryable<TEntity> query = _dbSet;
        if (include != null)
            query = include(query);
        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();
}
