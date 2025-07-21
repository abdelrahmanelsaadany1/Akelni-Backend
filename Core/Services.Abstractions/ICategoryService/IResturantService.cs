using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Abstractions.ICategoryService
{
    public interface IResturantService
    {
        Task AddResturantAsync(Restaurant Restaurant);
    }
}
