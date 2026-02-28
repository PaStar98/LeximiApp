using Leximi.Application.Services;
using Leximi.Application.Interfaces.Persistence;
using Leximi.Domain.Entities;
using Leximi.Application.DTOs;
using Moq;
using Xunit;
using FluentAssertions;

namespace Leximi.Application.UnitTests.Services;

public class AttemptServiceTests
{
    private readonly Mock<IAttemptRepository> _attemptRepositoryMock;
    private readonly Mock<ILearningSetRepository> _setRepositoryMock;
    private readonly Mock<IAnswerRepository> _answerRepositoryMock;
    private readonly Mock<IUserAnswerRepository> _userAnswerRepositoryMock;
    private readonly AttemptService _service;

    public AttemptServiceTests()
    {
        _attemptRepositoryMock = new Mock<IAttemptRepository>();
        _setRepositoryMock = new Mock<ILearningSetRepository>();
        _answerRepositoryMock = new Mock<IAnswerRepository>();
        _userAnswerRepositoryMock = new Mock<IUserAnswerRepository>();
        _service = new AttemptService(
            _attemptRepositoryMock.Object, 
            _setRepositoryMock.Object, 
            _answerRepositoryMock.Object,
            _userAnswerRepositoryMock.Object);
    }

    [Fact]
    public async Task StartAttemptAsync_Should_Create_Attempt_When_Set_Exists()
    {
        // Arrange
        var setId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var learningSet = new LearningSet { Id = setId, Title = "Test Set", Type = Domain.Enums.SetType.Flashcards, OwnerId = userId };

        _setRepositoryMock.Setup(r => r.GetByIdAsync(setId))
            .ReturnsAsync(learningSet);

        // Act
        var result = await _service.StartAttemptAsync(setId, userId);

        // Assert
        result.Should().NotBeNull();
        result.SetId.Should().Be(setId);
        _attemptRepositoryMock.Verify(r => r.AddAsync(It.IsAny<LearningSetAttempt>()), Times.Once);
        _attemptRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SubmitAnswerAsync_Should_Set_Score_Using_CountCorrectAnswers()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var answerId = Guid.NewGuid();
        var learningSet = new LearningSet { Type = Domain.Enums.SetType.Quiz, Title = "Title" };
        var attempt = new LearningSetAttempt { Id = attemptId, LearningSet = learningSet, UserAnswers = new List<UserAnswer>() };
        var answer = new Answer { Id = answerId, IsCorrect = true, Content = "Correct" };

        _attemptRepositoryMock.Setup(r => r.GetWithUserAnswersAsync(attemptId))
            .ReturnsAsync(attempt);
        _answerRepositoryMock.Setup(r => r.GetByIdAsync(answerId))
            .ReturnsAsync(answer);
        _attemptRepositoryMock.Setup(r => r.CountCorrectAnswersAsync(attemptId))
            .ReturnsAsync(1);

        var request = new SubmitAnswerDto(questionId, answerId, null);

        // Act
        var result = await _service.SubmitAnswerAsync(attemptId, request);

        // Assert
        result.Score.Should().Be(1);
        attempt.Score.Should().Be(1);
        _userAnswerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<UserAnswer>()), Times.Once);
        _userAnswerRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        _attemptRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task FinishAttemptAsync_Should_Calculate_Score_From_Database()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        var learningSet = new LearningSet { Type = Domain.Enums.SetType.Quiz, Title = "Title" };
        var attempt = new LearningSetAttempt { Id = attemptId, LearningSet = learningSet };

        _attemptRepositoryMock.Setup(r => r.GetWithUserAnswersAsync(attemptId))
            .ReturnsAsync(attempt);
        _attemptRepositoryMock.Setup(r => r.CountCorrectAnswersAsync(attemptId))
            .ReturnsAsync(5);

        // Act
        var result = await _service.FinishAttemptAsync(attemptId);

        // Assert
        result.Score.Should().Be(5);
        attempt.FinishedAt.Should().NotBeNull();
        _attemptRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
