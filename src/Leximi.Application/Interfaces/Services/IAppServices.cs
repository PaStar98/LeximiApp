using Leximi.Application.DTOs;

namespace Leximi.Application.Interfaces.Services;

public interface IIdentityService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<UserDto> GetCurrentUserAsync(Guid userId);
}

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto request);
}

public interface ILearningSetService
{
    Task<LearningSetDetailsDto> GetSetByIdAsync(Guid id);
    Task<IEnumerable<LearningSetDto>> GetSetsByCategoryAsync(Guid categoryId);
    Task<LearningSetDto> CreateSetAsync(CreateLearningSetDto request, Guid userId);
    Task<LearningSetDetailsDto> UpdateSetAsync(Guid id, UpdateLearningSetDto request, Guid userId);
}

public interface IAttemptService
{
    Task<AttemptDto> StartAttemptAsync(Guid setId, Guid userId);
    Task<AttemptDto> SubmitAnswerAsync(Guid attemptId, SubmitAnswerDto request);
    Task<AttemptDto> FinishAttemptAsync(Guid attemptId);
    Task<AttemptDto> GetAttemptByIdAsync(Guid attemptId);
    Task<IEnumerable<AttemptHistoryDto>> GetUserHistoryAsync(Guid userId);
}
