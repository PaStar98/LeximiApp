using Leximi.Application.DTOs;
using Leximi.Application.Interfaces.Persistence;
using Leximi.Application.Interfaces.Services;
using Leximi.Domain.Entities;

namespace Leximi.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _repository.GetAllAsync();
        return categories.Select(c => new CategoryDto(c.Id, c.Name, c.Description));
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto request)
    {
        var category = new Category { Name = request.Name, Description = request.Description };
        await _repository.AddAsync(category);
        await _repository.SaveChangesAsync();
        return new CategoryDto(category.Id, category.Name, category.Description);
    }
}

public class LearningSetService : ILearningSetService
{
    private readonly ILearningSetRepository _repository;

    public LearningSetService(ILearningSetRepository repository)
    {
        _repository = repository;
    }

    public async Task<LearningSetDetailsDto> GetSetByIdAsync(Guid id)
    {
        var set = await _repository.GetWithItemsAsync(id);
        if (set == null) throw new Exception("Set not found");

        var items = set.Items.Select(i => new LearningItemDto(
            i.Id,
            i.Question != null ? new QuestionDto(
                i.Question.Id, 
                i.Question.Content, 
                i.Question.Answers.Select(a => new AnswerDto(a.Id, a.Content, a.IsCorrect)).ToList()
            ) : null,
            i.Flashcard != null ? new FlashcardDto(i.Flashcard.Front, i.Flashcard.Back) : null
        )).ToList();

        return new LearningSetDetailsDto(set.Id, set.Title, set.Description, set.Type.ToString(), items);
    }

    public async Task<IEnumerable<LearningSetDto>> GetSetsByCategoryAsync(Guid categoryId)
    {
        var sets = await _repository.GetAllAsync();
        return sets.Where(s => s.CategoryId == categoryId)
                   .Select(s => new LearningSetDto(s.Id, s.Title, s.Description, s.Type.ToString()));
    }

    public async Task<LearningSetDto> CreateSetAsync(CreateLearningSetDto request, Guid userId)
    {
        if (!Enum.TryParse<Leximi.Domain.Enums.SetType>(request.Type, out var type))
            throw new Exception("Invalid set type");

        var set = new LearningSet
        {
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            OwnerId = userId,
            Type = type
        };

        await _repository.AddAsync(set);
        await _repository.SaveChangesAsync();
        return new LearningSetDto(set.Id, set.Title, set.Description, set.Type.ToString());
    }
}
