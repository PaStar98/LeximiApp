namespace Leximi.Application.DTOs;

public record RegisterRequestDto(string Email, string Username, string Password);
public record LoginRequestDto(string Email, string Password);
public record AuthResponseDto(string Token, string Username, string Email);

public record UserDto(Guid Id, string Username, string Email);

public record CategoryDto(Guid Id, string Name, string? Description);
public record CreateCategoryDto(string Name, string? Description);

public record LearningSetDto(Guid Id, string Title, string? Description, string Type);
public record CreateLearningSetDto(string Title, string? Description, Guid CategoryId, string Type);

public record AttemptDto(Guid Id, Guid SetId, DateTime StartedAt, DateTime? FinishedAt, int Score);
public record SubmitAnswerDto(Guid QuestionId, Guid? AnswerId, string? ProvidedText);
