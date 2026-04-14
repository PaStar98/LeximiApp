using Leximi.Application.DTOs;
using Leximi.Application.Interfaces.Persistence;
using Leximi.Application.Interfaces.Services;
using Leximi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Leximi.Application.Services;

public class AttemptService : IAttemptService
{
    private readonly IAttemptRepository _attemptRepository;
    private readonly ILearningSetRepository _setRepository;

    private readonly IAnswerRepository _answerRepository;
    private readonly IUserAnswerRepository _userAnswerRepository;

    public AttemptService(
        IAttemptRepository attemptRepository, 
        ILearningSetRepository setRepository, 
        IAnswerRepository answerRepository,
        IUserAnswerRepository userAnswerRepository)
    {
        _attemptRepository = attemptRepository;
        _setRepository = setRepository;
        _answerRepository = answerRepository;
        _userAnswerRepository = userAnswerRepository;
    }

    public async Task<AttemptDto> StartAttemptAsync(Guid setId, Guid userId)
    {
        var set = await _setRepository.GetByIdAsync(setId);
        if (set == null) throw new KeyNotFoundException("Set not found");

        var attempt = new LearningSetAttempt
        {
            LearningSetId = setId,
            UserId = userId,
            StartedAt = DateTime.UtcNow,
            Score = 0
        };

        await _attemptRepository.AddAsync(attempt);
        await _attemptRepository.SaveChangesAsync();

        return new AttemptDto(attempt.Id, attempt.LearningSetId, attempt.StartedAt, attempt.FinishedAt, attempt.Score);
    }

    public async Task<AttemptDto> SubmitAnswerAsync(Guid attemptId, SubmitAnswerDto request)
    {
        var attempt = await _attemptRepository.GetWithUserAnswersAsync(attemptId);
        if (attempt == null) throw new KeyNotFoundException("Attempt not found");

        bool isQuestionBased = attempt.LearningSet.Type != Leximi.Domain.Enums.SetType.Flashcards;
        bool isCorrect = false;

        if (request.AnswerId.HasValue && isQuestionBased)
        {
            var answer = await _answerRepository.GetByIdAsync(request.AnswerId.Value);
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

        await _userAnswerRepository.AddAsync(userAnswer);
        await _userAnswerRepository.SaveChangesAsync();
        
        if (isQuestionBased)
        {
            // Robust score update: count from DB
            attempt.Score = await _attemptRepository.CountCorrectAnswersAsync(attemptId);
            try 
            {
                await _attemptRepository.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // If collision, reload and retry once
                var retryAttempt = await _attemptRepository.GetByIdAsync(attemptId);
                if (retryAttempt != null)
                {
                    retryAttempt.Score = await _attemptRepository.CountCorrectAnswersAsync(attemptId);
                    await _attemptRepository.SaveChangesAsync();
                }
            }
        }
        else
        {
            attempt.Score = 0;
            await _attemptRepository.SaveChangesAsync(); // Save attempt score even if 0 for flashcards
        }

        return new AttemptDto(attempt.Id, attempt.LearningSetId, attempt.StartedAt, attempt.FinishedAt, attempt.Score);
    }

    public async Task<AttemptDto> FinishAttemptAsync(Guid attemptId)
    {
        var attempt = await _attemptRepository.GetWithUserAnswersAsync(attemptId);
        if (attempt == null) throw new KeyNotFoundException("Attempt not found");

        attempt.FinishedAt = DateTime.UtcNow;
        bool isQuestionBased = attempt.LearningSet.Type != Leximi.Domain.Enums.SetType.Flashcards;
        
        if (isQuestionBased)
        {
            attempt.Score = await _attemptRepository.CountCorrectAnswersAsync(attemptId);
        }
        else
        {
            attempt.Score = 0;
        }
        
        try 
        {
            await _attemptRepository.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var retryAttempt = await _attemptRepository.GetWithUserAnswersAsync(attemptId);
            if (retryAttempt != null)
            {
                retryAttempt.FinishedAt = DateTime.UtcNow;
                retryAttempt.Score = await _attemptRepository.CountCorrectAnswersAsync(attemptId);
                await _attemptRepository.SaveChangesAsync();
            }
        }

        return new AttemptDto(attempt.Id, attempt.LearningSetId, attempt.StartedAt, attempt.FinishedAt, attempt.Score);
    }

    public async Task<AttemptDto> GetAttemptByIdAsync(Guid attemptId)
    {
        var attempt = await _attemptRepository.GetByIdAsync(attemptId);
        if (attempt == null) throw new KeyNotFoundException("Attempt not found");
        return new AttemptDto(attempt.Id, attempt.LearningSetId, attempt.StartedAt, attempt.FinishedAt, attempt.Score);
    }

    public async Task<IEnumerable<AttemptHistoryDto>> GetUserHistoryAsync(Guid userId)
    {
        var attempts = await _attemptRepository.GetByUserIdAsync(userId);
        return attempts.Select(a => {
            string typeStr = a.LearningSet.Type.ToString();
            return new AttemptHistoryDto(
                a.Id, 
                a.LearningSetId, 
                a.LearningSet.Title, 
                typeStr, 
                a.StartedAt, 
                a.FinishedAt, 
                a.Score);
        });
    }
}
