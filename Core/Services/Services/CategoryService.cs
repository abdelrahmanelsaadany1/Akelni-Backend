using Domain.Contracts;
using Domain.Entities;
using Services.Abstractions.ICategoryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.CategoryService
{
    public class CategoryService : ICategoryService
    {
        private readonly IGenericRepository<Category> _categoryRepository;

        public CategoryService(IGenericRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task AddCategoryAsync(Category category)
        {
            var existingCategories = await _categoryRepository.GetAllAsync();

            if (existingCategories.Any(c => c.Name == category.Name))
                throw new Exception("Category name already exists.");

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new Exception("Category not found.");
            return category;
        }

        public async Task<Category> GetCategoryByNameAsync(string name)
        {
            var categories = await _categoryRepository.GetAllAsync();
            var category = categories.FirstOrDefault(c => c.Name == name);

            if (category == null)
                throw new Exception("Category not found.");
            return category;
        }

        public async Task UpdateCategoryAsync(int id, string newName)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new Exception("Category not found.");

            var allCategories = await _categoryRepository.GetAllAsync();
            if (allCategories.Any(c => c.Name == newName && c.Id != id))
                throw new Exception("Another category with this name already exists.");

            category.Name = newName;
            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new Exception("Category not found.");

            _categoryRepository.Delete(category);
            await _categoryRepository.SaveChangesAsync();
        }
    }
}
