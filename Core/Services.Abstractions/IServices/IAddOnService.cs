using Domain.Dtos.AddOnDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Abstractions.IServices
{
    public interface IAddOnService
    {
        Task<IEnumerable<ReturnDto>> GetAllAsync();
        Task<ReturnDto> GetByIdAsync(int id);
        Task<ReturnDto> AddAsync(AddOnDto dto); 
        Task<ReturnDto> UpdateAsync(int id, AddOnDto dto);
        Task DeleteAsync(int id);
    }
}
