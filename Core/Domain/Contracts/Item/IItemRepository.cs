using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Domain.Entities; // Assuming BaseEntity is defined here

namespace Domain.Contracts.Item
{
    public interface IItemRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : BaseEntity
    {
        Task<IEnumerable<TEntity>> GetAllWithIncludesAsync(string includeProperties);
        Task<IEnumerable<TEntity>> GetAllWithIncludesAsync(
                            Expression<Func<TEntity, bool>> predicate,
                            string? includeProperties);

        Task<TEntity?> GetByIdWithIncludesAsync(int id, string includeProperties);
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task<bool> ExistsAsync(int id);
    }
}
