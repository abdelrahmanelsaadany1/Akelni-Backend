using Domain.Contracts;
using Domain.Contracts.SieveProcessor;
using Domain.Entities;
using Services.Abstractions.IServices;
using Sieve.Services;

namespace Services.Services
{
    public class GenericService<TEntity> : IGenericService<TEntity> where TEntity : BaseEntity
    {
        private readonly IGenericRepository<TEntity> _repository;
        private readonly SieveProcessor _sieveProcessor;

        public GenericService(
            IGenericRepository<TEntity> repository,
            SieveProcessor sieveProcessor)
        {
            _repository = repository;
            _sieveProcessor = sieveProcessor;
        }

        // Defaults to pagination ON
        public IQueryable<TEntity> GetAllSieveAsync(CustomSieveModel sieveModel)
            => GetAllSieveAsync(sieveModel, applyPagination: true);

        // Overload controlling pagination (implemented by stripping Page/PageSize)
        public IQueryable<TEntity> GetAllSieveAsync(CustomSieveModel sieveModel, bool applyPagination)
        {
            sieveModel ??= new CustomSieveModel();

            var baseQuery = _repository.GetAllQuerable();

            if (!applyPagination)
            {
                // Clone without pagination values
                var noPageModel = new CustomSieveModel
                {
                    Filters = sieveModel.Filters,
                    Sorts = sieveModel.Sorts,
                    Page = null,
                    PageSize = null
                };
                return _sieveProcessor.Apply(noPageModel, baseQuery);
            }

            return _sieveProcessor.Apply(sieveModel, baseQuery);
        }

        public List<T> CustomPagination<T>(IEnumerable<T> source, int page, int pageSize)
            => source.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    }
}