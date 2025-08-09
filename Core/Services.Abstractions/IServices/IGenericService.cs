using Domain.Contracts.SieveProcessor;
using Domain.Entities;

namespace Services.Abstractions.IServices
{
    public interface IGenericService<TEntity> where TEntity : BaseEntity
    {
        IQueryable<TEntity> GetAllSieveAsync(CustomSieveModel sieveModel);
        List<T> CustomPagination<T>(IEnumerable<T> source, int page, int pageSize);
    }
}