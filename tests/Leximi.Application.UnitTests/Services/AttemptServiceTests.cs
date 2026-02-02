using Leximi.Application.Services;
using Leximi.Application.Interfaces.Persistence;
using Leximi.Domain.Entities;
using Moq;
using Xunit;
using FluentAssertions;

namespace Leximi.Application.UnitTests.Services;

public class AttemptServiceTests
{
    private readonly Mock<IAttemptRepository> _attemptRepositoryMock;
    private readonly Mock<ILearningSetRepository> _setRepositoryMock;
    private readonly AttemptService _service;

    public AttemptServiceTests()
    {
        _attemptRepositoryMock = new Mock<IAttemptRepository>();
        _setRepositoryMock = new Mock<ILearningSetRepository>();
        _service = new AttemptService(_attemptRepositoryMock.Object, _setRepositoryMock.Object);
    }

    [Fact]
    public async Task StartAttemptAsync_Should_Create_Attempt_When_Set_Exists()
    {
        // Arrange
        var setId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var learningSet = new LearningSet { Id = setId, Title = "Test Set" };

        _setRepositoryMock.Setup(r => r.GetByIdAsync(setId))
            .ReturnsAsync(learningSet);

        // Act
        var result = await _service.StartAttemptAsync(setId, userId);

        // Assert
        result.Should().NotBeNull();
        result.LearningSetId.Should().Be(setId);
        _attemptRepositoryMock.Verify(r => r.AddAsync(It.IsAny<LearningSetAttempt>()), Times.Once);
        _attemptRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task StartAttemptAsync_Should_Throw_Exception_When_Set_Does_Not_Exist()
    {
        // Arrange
        var setId = Guid.NewGuid();
        _setRepositoryMock.Setup(r => r.GetByIdAsync(setId))
            .ReturnsAsync((LearningSet?)null);

        // Act
        var act = () => _service.StartAttemptAsync(setId, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Set not found");
    }
}
