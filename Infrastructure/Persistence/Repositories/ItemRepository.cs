using Domain.Contracts.Item;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

public class ItemRepository<TEntity> : GenericRepository<TEntity>, IItemRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly FoodCourtDbContext _context;

    public ItemRepository(FoodCourtDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TEntity>> GetAllWithIncludesAsync(string includeProperties)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();

        foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProp.Trim());
        }

        return await query.ToListAsync();
    }

    public async Task<TEntity?> GetByIdWithIncludesAsync(int id, string includeProperties)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();

        foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProp.Trim());
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _context.Set<TEntity>().Where(predicate).ToListAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Set<TEntity>().AnyAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<TEntity>> GetAllWithIncludesAsync(
        Expression<Func<TEntity, bool>> predicate,
        string? includeProperties = null)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();

        if (!string.IsNullOrWhiteSpace(includeProperties))
        {
            foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp.Trim());
            }
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync();
    }

}
