using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using Persistence.Data;
using System;
using System.Linq.Expressions;

public class OrderRepository<TEntity> : IExtendedRepository<TEntity> where TEntity : BaseEntity
{
    private readonly FoodCourtDbContext _context;

    public OrderRepository(FoodCourtDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync() =>
        await _context.Set<TEntity>().ToListAsync();

    public async Task<TEntity> GetByIdAsync(int id) =>
        await _context.Set<TEntity>().FindAsync(id);

    // Revert: Call SaveChanges to get the entity ID immediately
    public async Task AddAsync(TEntity entity)
    {
        await _context.Set<TEntity>().AddAsync(entity);
        await _context.SaveChangesAsync(); // Keep this to get the ID
    }

    public void Update(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
    }

    public void Delete(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
    }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    // Extended methods
    public async Task<TEntity> GetByIdAsync(int id, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        if (include != null)
            query = include(query);
        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();
        if (filter != null)
            query = query.Where(filter);
        if (include != null)
            query = include(query);
        if (orderBy != null)
            query = orderBy(query);
        return await query.ToListAsync();
    }

    public async Task UpdateAsync(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Set<TEntity>().FindAsync(id);
        if (entity != null)
        {
            _context.Set<TEntity>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public IQueryable<TEntity> GetAllQuerable()
    {
        throw new NotImplementedException();
    }
}

