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
            !string.IsNullOrEmpty(i.QuestionContent) ? new QuestionDto(
                Guid.Empty, // QuestionId is gone
                i.QuestionContent, 
                i.Answers.Select(a => new AnswerDto(a.Id, a.Content, a.IsCorrect)).ToList()
            ) : null,
            !string.IsNullOrEmpty(i.FlashcardFront) ? new FlashcardDto(i.FlashcardFront, i.FlashcardBack ?? "") : null
        )).ToList();

        return new LearningSetDetailsDto(set.Id, set.Title, set.Description, set.CategoryId, set.Type.ToString(), items);
    }

    public async Task<IEnumerable<LearningSetDto>> GetSetsByCategoryAsync(Guid categoryId)
    {
        var sets = await _repository.GetAllAsync();
        return sets.Where(s => s.CategoryId == categoryId)
                   .Select(s => new LearningSetDto(s.Id, s.Title, s.Description, s.CategoryId, s.Type.ToString()));
    }

    public async Task<LearningSetDto> CreateSetAsync(CreateLearningSetDto request, Guid userId)
    {
        if (!Enum.TryParse<Leximi.Domain.Enums.SetType>(request.Type, true, out var type))
            throw new ArgumentException($"Invalid set type: {request.Type}");

        var set = new LearningSet
        {
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            OwnerId = userId,
            Type = type,
            Items = new List<LearningItem>()
        };

        if (request.Items != null && request.Items.Any())
        {
            SyncItems(set, request.Items);
        }

        await _repository.AddAsync(set);
        await _repository.SaveChangesAsync();
        return new LearningSetDto(set.Id, set.Title, set.Description, set.CategoryId, set.Type.ToString());
    }

    public async Task<LearningSetDetailsDto> UpdateSetAsync(Guid id, UpdateLearningSetDto request, Guid userId)
    {
        int maxRetries = 3;
        int currentRetry = 0;

        while (true)
        {
            try
            {
                var set = await _repository.GetWithItemsForUpdateAsync(id);
                if (set == null) throw new KeyNotFoundException("Set not found");
                if (set.OwnerId != userId) throw new UnauthorizedAccessException("You are not the owner of this set");

                set.Title = request.Title;
                set.Description = request.Description;
                set.CategoryId = request.CategoryId;

                if (Enum.TryParse<Leximi.Domain.Enums.SetType>(request.Type, true, out var newType))
                {
                    set.Type = newType;
                }
                
                SyncItems(set, request.Items);

                await _repository.SaveChangesAsync();
                break;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException) when (currentRetry < maxRetries)
            {
                _repository.ClearTracker();
                currentRetry++;
                await Task.Delay(100 * currentRetry);
            }
        }
        
        return await GetSetByIdAsync(id);
    }

    private void SyncItems(LearningSet set, List<UpdateLearningItemDto> requestItems)
    {
        requestItems ??= new List<UpdateLearningItemDto>();
        bool isQuestionBased = set.Type != Leximi.Domain.Enums.SetType.Flashcards;
        
        var itemsToRemove = set.Items
            .Where(i => !i.IsDeleted)
            .Where(i => !requestItems.Any(ri => ri.Id == i.Id))
            .ToList();

        foreach (var item in itemsToRemove)
        {
            item.IsDeleted = true;
            foreach(var a in item.Answers.Where(a => !a.IsDeleted)) a.IsDeleted = true;
        }

        foreach (var itemDto in requestItems)
        {
            var existingItem = itemDto.Id.HasValue ? set.Items.FirstOrDefault(i => i.Id == itemDto.Id.Value) : null;

            if (existingItem != null)
            {
                existingItem.IsDeleted = false;
                
                if (isQuestionBased)
                {
                    existingItem.QuestionContent = itemDto.QuestionContent;
                    existingItem.FlashcardFront = null;
                    existingItem.FlashcardBack = null;
                    
                    if (itemDto.Answers != null)
                    {
                        var answersToRemove = existingItem.Answers
                            .Where(a => !a.IsDeleted)
                            .Where(a => !itemDto.Answers.Any(ra => ra.Id == a.Id))
                            .ToList();
                        foreach(var a in answersToRemove) a.IsDeleted = true;

                        foreach(var answerDto in itemDto.Answers)
                        {
                            var existingAnswer = answerDto.Id.HasValue 
                                ? existingItem.Answers.FirstOrDefault(a => a.Id == answerDto.Id.Value) 
                                : null;

                            if (existingAnswer != null)
                            {
                                existingAnswer.Content = answerDto.Content;
                                existingAnswer.IsCorrect = answerDto.IsCorrect;
                            }
                            else
                            {
                                existingItem.Answers.Add(new Answer 
                                { 
                                    LearningItemId = existingItem.Id,
                                    Content = answerDto.Content, 
                                    IsCorrect = answerDto.IsCorrect 
                                });
                            }
                        }
                    }
                }
                else
                {
                    existingItem.FlashcardFront = itemDto.FlashcardFront;
                    existingItem.FlashcardBack = itemDto.FlashcardBack;
                    existingItem.QuestionContent = null;
                    // Flashcards don't use Answers, mark all as deleted if any
                    foreach(var a in existingItem.Answers) a.IsDeleted = true;
                }
            }
            else
            {
                // Only add if there's actual content
                bool hasContent = isQuestionBased 
                    ? !string.IsNullOrWhiteSpace(itemDto.QuestionContent) 
                    : !string.IsNullOrWhiteSpace(itemDto.FlashcardFront);

                if (hasContent)
                {
                    var newItem = new LearningItem 
                    { 
                        LearningSetId = set.Id,
                        QuestionContent = isQuestionBased ? itemDto.QuestionContent : null,
                        FlashcardFront = !isQuestionBased ? itemDto.FlashcardFront : null,
                        FlashcardBack = !isQuestionBased ? itemDto.FlashcardBack : null,
                    };
                    
                    if (isQuestionBased && itemDto.Answers != null)
                    {
                        foreach (var a in itemDto.Answers.Where(a => !string.IsNullOrWhiteSpace(a.Content)))
                        {
                            newItem.Answers.Add(new Answer 
                            { 
                                LearningItemId = newItem.Id, 
                                Content = a.Content, 
                                IsCorrect = a.IsCorrect 
                            });
                        }
                    }
                    
                    set.Items.Add(newItem);
                }
            }
        }
    }

}
