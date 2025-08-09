using Domain.Contracts;
using Domain.Contracts.SieveProcessor;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions.IServices;
using Sieve.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class GenericService<TEntity> : IGenericService<TEntity> where TEntity : BaseEntity
    {
        IGenericRepository<TEntity> _repository;
        SieveProcessor _sieveProcessor;
        public GenericService(
            IGenericRepository<TEntity> repository,
            SieveProcessor sieveProcessor)
        {

            _repository = repository;
            _sieveProcessor = sieveProcessor;
        }
        public IQueryable<TEntity> GetAllSieveAsync(CustomSieveModel sieveModel)
        {
            var entityQuery = _repository.GetAllQuerable();
            var filteredResult = _sieveProcessor.Apply(sieveModel, entityQuery, applyPagination: true);
            return filteredResult;
        }
        public List<T> CustomPagination<T>(IEnumerable<T> source, int page, int pageSize) {
            return source.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }
    }
}