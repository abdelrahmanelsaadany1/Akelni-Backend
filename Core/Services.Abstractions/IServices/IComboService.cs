using Domain.Dtos.ComboDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Abstractions.IServices
{
    public interface IComboService
    {
        Task<IEnumerable<ReturnDto>> GetAllAsync();
        Task<ReturnDto?> GetByIdAsync(int id);
        Task AddAsync(ComboDto dto);
        Task UpdateAsync(int id, ComboDto dto);
        Task DeleteAsync(int id);
    }
}
