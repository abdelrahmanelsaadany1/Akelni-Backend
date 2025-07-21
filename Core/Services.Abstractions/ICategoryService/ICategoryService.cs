using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Abstractions.ICategoryService
{
    public interface ICategoryService
    {
        Task AddCategoryAsync(Category category);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category> GetCategoryByIdAsync(int id);
        Task<Category> GetCategoryByNameAsync(string name);
        Task UpdateCategoryAsync(int id, string newName);
        Task DeleteCategoryAsync(int id);
    }
}
