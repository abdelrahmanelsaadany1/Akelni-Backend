using Domain.Contracts;
using Domain.Entities;
using Services.Abstractions.ICategoryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.CategoryService
{
    public class OrderService : IOrderService
    {
        private readonly IGenericRepository<Order> _OrderRepository;

        public OrderService(IGenericRepository<Order> OrderRepository)
        {
            _OrderRepository = OrderRepository;
        }
        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _OrderRepository.GetAllAsync();
        }
    }
}
