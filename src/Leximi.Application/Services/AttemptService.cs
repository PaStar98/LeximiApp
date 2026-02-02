using Leximi.Application.DTOs;
using Leximi.Application.Interfaces.Persistence;
using Leximi.Application.Interfaces.Services;
using Leximi.Domain.Entities;

namespace Leximi.Application.Services;

public class AttemptService : IAttemptService
{
    private readonly IAttemptRepository _attemptRepository;
    private readonly ILearningSetRepository _setRepository;

    public AttemptService(IAttemptRepository attemptRepository, ILearningSetRepository setRepository)
    {
        _attemptRepository = attemptRepository;
        _setRepository = setRepository;
    }

    public async Task<AttemptDto> StartAttemptAsync(Guid setId, Guid userId)
    {
        var set = await _setRepository.GetByIdAsync(setId);
        if (set == null) throw new Exception("Set not found");

        var attempt = new LearningSetAttempt
        {
            LearningSetId = setId,
            UserId = userId,
            StartedAt = DateTime.UtcNow
        };

        await _attemptRepository.AddAsync(attempt);
        await _attemptRepository.SaveChangesAsync();

        return new AttemptDto(attempt.Id, attempt.LearningSetId, attempt.StartedAt, attempt.FinishedAt, attempt.Score);
    }

    public async Task<AttemptDto> SubmitAnswerAsync(Guid attemptId, SubmitAnswerDto request)
    {
        var attempt = await _attemptRepository.GetByIdAsync(attemptId);
        if (attempt == null) throw new Exception("Attempt not found");

        // Logic for checking correctness would go here
        // For now, just recording the answer
        var userAnswer = new UserAnswer
        {
            AttemptId = attemptId,
            QuestionId = request.QuestionId,
            AnswerId = request.AnswerId,
            ProvidedText = request.ProvidedText
        };

        attempt.UserAnswers.Add(userAnswer);
        await _attemptRepository.SaveChangesAsync();

        return new AttemptDto(attempt.Id, attempt.LearningSetId, attempt.StartedAt, attempt.FinishedAt, attempt.Score);
    }

    public async Task<AttemptDto> FinishAttemptAsync(Guid attemptId)
    {
        var attempt = await _attemptRepository.GetByIdAsync(attemptId);
        if (attempt == null) throw new Exception("Attempt not found");

        attempt.FinishedAt = DateTime.UtcNow;
        // Calculate score logic...
        
        await _attemptRepository.SaveChangesAsync();
        return new AttemptDto(attempt.Id, attempt.LearningSetId, attempt.StartedAt, attempt.FinishedAt, attempt.Score);
    }

    public async Task<IEnumerable<AttemptDto>> GetUserHistoryAsync(Guid userId)
    {
        var attempts = await _attemptRepository.GetByUserIdAsync(userId);
        return attempts.Select(a => new AttemptDto(a.Id, a.LearningSetId, a.StartedAt, a.FinishedAt, a.Score));
    }
}
