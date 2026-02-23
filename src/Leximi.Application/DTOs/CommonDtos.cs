namespace Leximi.Application.DTOs;

public record RegisterRequestDto(string Email, string Username, string Password);
public record LoginRequestDto(string Email, string Password);
public record AuthResponseDto(string Token, string Username, string Email);

public record UserDto(Guid Id, string Username, string Email);

public record CategoryDto(Guid Id, string Name, string? Description);
public record CreateCategoryDto(string Name, string? Description);

public record LearningSetDto(Guid Id, string Title, string? Description, Guid CategoryId, string Type);
public record CreateLearningSetDto(string Title, string? Description, Guid CategoryId, string Type);

public record AttemptDto(Guid Id, Guid SetId, DateTime StartedAt, DateTime? FinishedAt, int Score);
public record AttemptHistoryDto(Guid Id, Guid SetId, string SetTitle, DateTime StartedAt, DateTime? FinishedAt, int Score);
public record SubmitAnswerDto(Guid QuestionId, Guid? AnswerId, string? ProvidedText);

public record AnswerDto(Guid Id, string Content, bool IsCorrect);
public record QuestionDto(Guid Id, string Content, List<AnswerDto> Answers);
public record FlashcardDto(string Front, string Back);
public record LearningItemDto(Guid Id, QuestionDto? Question, FlashcardDto? Flashcard);
public record LearningSetDetailsDto(Guid Id, string Title, string? Description, Guid CategoryId, string Type, List<LearningItemDto> Items) 
    : LearningSetDto(Id, Title, Description, CategoryId, Type);

public record UpdateLearningItemDto(Guid? Id, string? QuestionContent, List<AnswerDto>? Answers, string? FlashcardFront, string? FlashcardBack);

public record UpdateLearningSetDto(string Title, string? Description, string Type, List<UpdateLearningItemDto> Items);
