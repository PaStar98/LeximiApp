using Leximi.Application.DTOs;
using Leximi.Application.Interfaces.Persistence;
using Leximi.Application.Interfaces.Services;
using Leximi.Domain.Entities;
using Leximi.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Leximi.Application.Services;

public class AttemptService(
    IAttemptRepository attemptRepository,
    ILearningSetRepository setRepository,
    IAnswerRepository answerRepository,
    IUserAnswerRepository userAnswerRepository) : IAttemptService
{
    public async Task<AttemptDto> StartAttemptAsync(Guid setId, Guid userId)
    {
        var set = await setRepository.GetByIdAsync(setId);
        if (set == null) throw new KeyNotFoundException("Set not found");

        var attempt = new LearningSetAttempt
        {
            LearningSetId = setId,
            UserId = userId,
            StartedAt = DateTime.UtcNow,
            Score = 0
        };

        await attemptRepository.AddAsync(attempt);
        await attemptRepository.SaveChangesAsync();

        return new AttemptDto(attempt.Id, attempt.LearningSetId, attempt.StartedAt, attempt.FinishedAt, attempt.Score);
    }

    public async Task<AttemptDto> SubmitAnswerAsync(Guid attemptId, SubmitAnswerDto request)
    {
        var attempt = await attemptRepository.GetWithUserAnswersAsync(attemptId);
        if (attempt == null) throw new KeyNotFoundException("Attempt not found");

        bool isQuestionBased = attempt.LearningSet.Type != SetType.Flashcards;
        bool isCorrect = false;

        if (request.AnswerId.HasValue && isQuestionBased)
        {
            var answer = await answerRepository.GetByIdAsync(request.AnswerId.Value);
            if (answer != null)
            {
                isCorrect = answer.IsCorrect;
            }
        }

        var userAnswer = new UserAnswer
        {
            AttemptId = attemptId,
            LearningItemId = request.LearningItemId,
            AnswerId = request.AnswerId,
            ProvidedText = request.ProvidedText,
            IsCorrect = isCorrect
        };

        await userAnswerRepository.AddAsync(userAnswer);
        await userAnswerRepository.SaveChangesAsync();
        
        if (isQuestionBased)
        {
            attempt.Score = await attemptRepository.CountCorrectAnswersAsync(attemptId);
            try 
            {
                await attemptRepository.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var retryAttempt = await attemptRepository.GetByIdAsync(attemptId);
                if (retryAttempt != null)
                {
                    retryAttempt.Score = await attemptRepository.CountCorrectAnswersAsync(attemptId);
                    await attemptRepository.SaveChangesAsync();
                }
            }
        }
        else
        {
            attempt.Score = 0;
            await attemptRepository.SaveChangesAsync(); 
        }

        return new AttemptDto(attempt.Id, attempt.LearningSetId, attempt.StartedAt, attempt.FinishedAt, attempt.Score);
    }

    public async Task<AttemptDto> FinishAttemptAsync(Guid attemptId)
    {
        var attempt = await attemptRepository.GetWithUserAnswersAsync(attemptId);
        if (attempt == null) throw new KeyNotFoundException("Attempt not found");

        attempt.FinishedAt = DateTime.UtcNow;
        bool isQuestionBased = attempt.LearningSet.Type != SetType.Flashcards;
        
        if (isQuestionBased)
        {
            attempt.Score = await attemptRepository.CountCorrectAnswersAsync(attemptId);
        }
        else
        {
            attempt.Score = 0;
        }
        
        try 
        {
            await attemptRepository.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var retryAttempt = await attemptRepository.GetWithUserAnswersAsync(attemptId);
            if (retryAttempt != null)
            {
                retryAttempt.FinishedAt = DateTime.UtcNow;
                retryAttempt.Score = await attemptRepository.CountCorrectAnswersAsync(attemptId);
                await attemptRepository.SaveChangesAsync();
            }
        }

        return new AttemptDto(attempt.Id, attempt.LearningSetId, attempt.StartedAt, attempt.FinishedAt, attempt.Score);
    }

    public async Task<AttemptDto> GetAttemptByIdAsync(Guid attemptId)
    {
        var attempt = await attemptRepository.GetByIdAsync(attemptId);
        if (attempt == null) throw new KeyNotFoundException("Attempt not found");
        return new AttemptDto(attempt.Id, attempt.LearningSetId, attempt.StartedAt, attempt.FinishedAt, attempt.Score);
    }

    public async Task<IEnumerable<AttemptHistoryDto>> GetUserHistoryAsync(Guid userId)
    {
        var attempts = await attemptRepository.GetByUserIdAsync(userId);
        return attempts.Select(a => {
            string typeStr = a.LearningSet.Type.ToString();
            int maxScore = a.LearningSet.Items.Count(i => !i.IsDeleted);
            
            return new AttemptHistoryDto(
                a.Id, 
                a.LearningSetId, 
                a.LearningSet.Title, 
                typeStr, 
                a.StartedAt, 
                a.FinishedAt, 
                a.Score,
                maxScore);
        });
    }
}
