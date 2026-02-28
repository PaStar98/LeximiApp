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
        var set = await _repository.GetWithItemsAsync(id);
        if (set == null) throw new KeyNotFoundException("Set not found");
        if (set.OwnerId != userId) throw new UnauthorizedAccessException("You are not the owner of this set");

        set.Title = request.Title;
        set.Description = request.Description;
        
        SyncItems(set, request.Items);

        await _repository.SaveChangesAsync();
        return await GetSetByIdAsync(id);
    }

    private void SyncItems(LearningSet set, List<UpdateLearningItemDto> requestItems)
    {
        requestItems ??= new List<UpdateLearningItemDto>();
        
        // 1. Remove items not in request (only if set already has items and we have IDs in request)
        var itemsToRemove = set.Items.Where(i => !requestItems.Any(ri => ri.Id == i.Id)).ToList();
        foreach (var item in itemsToRemove)
        {
            set.Items.Remove(item);
        }

        // 2. Add or Update items
        foreach (var itemDto in requestItems)
        {
            var existingItem = itemDto.Id.HasValue ? set.Items.FirstOrDefault(i => i.Id == itemDto.Id.Value) : null;

            if (existingItem != null)
            {
                // Update
                if (set.Type == Leximi.Domain.Enums.SetType.Quiz && itemDto.QuestionContent != null)
                {
                    if (existingItem.Question == null) existingItem.Question = new Question { Content = itemDto.QuestionContent };
                    existingItem.Question.Content = itemDto.QuestionContent;
                    
                    // Sync Answers
                    if (itemDto.Answers != null)
                    {
                        var answersToRemove = existingItem.Question.Answers.Where(a => !itemDto.Answers.Any(ra => ra.Id == a.Id)).ToList();
                        foreach(var a in answersToRemove) existingItem.Question.Answers.Remove(a);

                        foreach(var answerDto in itemDto.Answers)
                        {
                            var existingAnswer = answerDto.Id.HasValue 
                                ? existingItem.Question.Answers.FirstOrDefault(a => a.Id == answerDto.Id.Value) 
                                : null;

                            if (existingAnswer != null)
                            {
                                existingAnswer.Content = answerDto.Content;
                                existingAnswer.IsCorrect = answerDto.IsCorrect;
                            }
                            else
                            {
                                existingItem.Question.Answers.Add(new Answer { Content = answerDto.Content, IsCorrect = answerDto.IsCorrect });
                            }
                        }
                    }
                }
                else if (set.Type == Leximi.Domain.Enums.SetType.Flashcards && itemDto.FlashcardFront != null)
                {
                    if (existingItem.Flashcard == null) 
                        existingItem.Flashcard = new Flashcard { Front = itemDto.FlashcardFront, Back = itemDto.FlashcardBack ?? "" };
                    
                    existingItem.Flashcard.Front = itemDto.FlashcardFront;
                    existingItem.Flashcard.Back = itemDto.FlashcardBack ?? "";
                }
            }
            else
            {
                // Add New
                var newItem = new LearningItem { LearningSetId = set.Id };
                if (set.Type == Leximi.Domain.Enums.SetType.Quiz)
                {
                    newItem.Question = new Question 
                    { 
                        Content = itemDto.QuestionContent ?? "",
                        Answers = itemDto.Answers?.Select(a => new Answer { Content = a.Content, IsCorrect = a.IsCorrect }).ToList() ?? new List<Answer>()
                    };
                }
                else
                {
                    newItem.Flashcard = new Flashcard
                    {
                        Front = itemDto.FlashcardFront ?? "",
                        Back = itemDto.FlashcardBack ?? ""
                    };
                }
                set.Items.Add(newItem);
            }
        }
    }
}
