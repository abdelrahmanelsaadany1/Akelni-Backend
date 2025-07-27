using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;

namespace Domain.Contracts
{
    public interface IGenericRepository<TEntity> where TEntity : BaseEntity
    {
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity> GetByIdAsync(int id);
        Task<TEntity> GetByIdAsync(int id, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null);


        Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);

        Task<int> SaveChangesAsync();
    }
}