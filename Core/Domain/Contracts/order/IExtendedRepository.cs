using Domain.Contracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

public interface IExtendedRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity> GetByIdAsync(int id, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);
    Task<IEnumerable<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>> filter = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null);

    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(int id);
}