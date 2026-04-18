using Leximi.Application.DTOs;
using Leximi.Application.Interfaces.Persistence;
using Leximi.Application.Interfaces.Services;
using Leximi.Domain.Entities;

namespace Leximi.Application.Services;

public class CategoryService(ICategoryRepository repository) : ICategoryService
{
    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await repository.GetAllAsync();
        return categories.Select(c => new CategoryDto(c.Id, c.Name, c.Description));
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto request)
    {
        var category = new Category { Name = request.Name, Description = request.Description };
        await repository.AddAsync(category);
        await repository.SaveChangesAsync();
        return new CategoryDto(category.Id, category.Name, category.Description);
    }
}
