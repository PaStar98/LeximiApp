using Leximi.Application.DTOs;
using Leximi.Application.Interfaces.Persistence;
using Leximi.Application.Interfaces.Services;
using Leximi.Domain.Entities;
using Leximi.Domain.Enums;

namespace Leximi.Application.Services;

public class LearningSetService(ILearningSetRepository repository) : ILearningSetService
{
    public async Task<LearningSetDetailsDto> GetSetByIdAsync(Guid id)
    {
        var set = await repository.GetWithItemsAsync(id);
        if (set == null) throw new Exception("Set not found");

        var items = set.Items.Select(i => new LearningItemDto(
            i.Id,
            !string.IsNullOrEmpty(i.QuestionContent) ? new QuestionDto(
                Guid.Empty,
                i.QuestionContent, 
                i.Answers.Select(a => new AnswerDto(a.Id, a.Content, a.IsCorrect)).ToList()
            ) : null,
            !string.IsNullOrEmpty(i.FlashcardFront) ? new FlashcardDto(i.FlashcardFront, i.FlashcardBack ?? "") : null
        )).ToList();

        return new LearningSetDetailsDto(set.Id, set.Title, set.Description, set.CategoryId, set.Type.ToString(), items, set.Owner.Username);
    }

    public async Task<IEnumerable<LearningSetDto>> GetSetsByCategoryAsync(Guid categoryId)
    {
        var sets = await repository.GetByCategoryAsync(categoryId);
        return sets.Select(s => new LearningSetDto(s.Id, s.Title, s.Description, s.CategoryId, s.Type.ToString(), s.Owner.Username));
    }

    public async Task<LearningSetDto> CreateSetAsync(CreateLearningSetDto request, Guid userId)
    {
        if (!Enum.TryParse<SetType>(request.Type, true, out var type))
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

        await repository.AddAsync(set);
        await repository.SaveChangesAsync();
        
        var createdSet = await repository.GetWithItemsAsync(set.Id);
        return new LearningSetDto(set.Id, set.Title, set.Description, set.CategoryId, set.Type.ToString(), createdSet?.Owner.Username ?? "Unknown");
    }

    public async Task<LearningSetDetailsDto> UpdateSetAsync(Guid id, UpdateLearningSetDto request, Guid userId)
    {
        int maxRetries = 3;
        int currentRetry = 0;

        while (true)
        {
            try
            {
                var set = await repository.GetWithItemsForUpdateAsync(id);
                if (set == null) throw new KeyNotFoundException("Set not found");
                if (set.OwnerId != userId) throw new UnauthorizedAccessException("You are not the owner of this set");

                set.Title = request.Title;
                set.Description = request.Description;
                set.CategoryId = request.CategoryId;

                if (Enum.TryParse<SetType>(request.Type, true, out var newType))
                {
                    set.Type = newType;
                }
                
                SyncItems(set, request.Items);

                await repository.SaveChangesAsync();
                break;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException) when (currentRetry < maxRetries)
            {
                repository.ClearTracker();
                currentRetry++;
                await Task.Delay(100 * currentRetry);
            }
        }
        
        return await GetSetByIdAsync(id);
    }

    private void SyncItems(LearningSet set, List<UpdateLearningItemDto> requestItems)
    {
        requestItems ??= new List<UpdateLearningItemDto>();
        bool isQuestionBased = set.Type != SetType.Flashcards;
        
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
                    foreach(var a in existingItem.Answers) a.IsDeleted = true;
                }
            }
            else
            {
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
