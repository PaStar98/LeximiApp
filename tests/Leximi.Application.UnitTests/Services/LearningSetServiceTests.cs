using Leximi.Application.Services;
using Leximi.Application.Interfaces.Persistence;
using Leximi.Domain.Entities;
using Leximi.Domain.Enums;
using Leximi.Application.DTOs;
using Moq;
using Xunit;
using FluentAssertions;

namespace Leximi.Application.UnitTests.Services;

public class LearningSetServiceTests
{
    private readonly Mock<ILearningSetRepository> _repositoryMock;
    private readonly LearningSetService _service;

    public LearningSetServiceTests()
    {
        _repositoryMock = new Mock<ILearningSetRepository>();
        _service = new LearningSetService(_repositoryMock.Object);
    }

    [Fact]
    public async Task UpdateSetAsync_Should_Add_New_Answer_When_Id_Is_Null()
    {
        // Arrange
        var setId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        
        var existingSet = new LearningSet 
        { 
            Id = setId, 
            Title = "Old Title", 
            Type = SetType.Quiz, 
            OwnerId = userId,
            Items = new List<LearningItem>
            {
                new LearningItem
                {
                    Id = itemId,
                    QuestionContent = "Old Question",
                    Answers = new List<Answer>()
                }
            }
        };

        _repositoryMock.Setup(r => r.GetWithItemsForUpdateAsync(setId))
            .ReturnsAsync(existingSet);
        _repositoryMock.Setup(r => r.GetWithItemsAsync(setId))
            .ReturnsAsync(existingSet);

        var updateDto = new UpdateLearningSetDto(
            "New Title",
            "Description",
            "Quiz",
            Guid.NewGuid(), // CategoryId
            new List<UpdateLearningItemDto>
            {
                new UpdateLearningItemDto(
                    itemId,
                    "Updated Question",
                    new List<AnswerDto>
                    {
                        new AnswerDto(null, "New Correct Answer", true)
                    },
                    null,
                    null
                )
            }
        );

        // Act
        await _service.UpdateSetAsync(setId, updateDto, userId);

        // Assert
        existingSet.Items.First().Answers.Should().HaveCount(1);
        existingSet.Items.First().Answers.First().Content.Should().Be("New Correct Answer");
        existingSet.Items.First().Answers.First().IsCorrect.Should().BeTrue();
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
    [Fact]
    public async Task CreateSetAsync_Should_Add_Items_When_Provided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        
        var createDto = new CreateLearningSetDto(
            "New Set",
            "Description",
            categoryId,
            "Flashcards",
            new List<UpdateLearningItemDto>
            {
                new UpdateLearningItemDto(null, null, null, "Front 1", "Back 1"),
                new UpdateLearningItemDto(null, null, null, "Front 2", "Back 2")
            }
        );

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<LearningSet>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateSetAsync(createDto, userId);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.Is<LearningSet>(s => 
            s.Title == "New Set" && 
            s.Items.Count == 2 &&
            s.Items.Any(i => i.FlashcardFront == "Front 1")
        )), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        result.Title.Should().Be("New Set");
    }

    [Fact]
    public async Task UpdateSetAsync_Should_Mark_Item_As_Deleted_When_Absent_In_Request()
    {
        // Arrange
        var setId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var itemIdToKeep = Guid.NewGuid();
        var itemIdToRemove = Guid.NewGuid();
        
        var existingSet = new LearningSet 
        { 
            Id = setId, 
            Title = "Test Set", 
            Type = SetType.Quiz, 
            OwnerId = userId,
            Items = new List<LearningItem>
            {
                new LearningItem { Id = itemIdToKeep, QuestionContent = "Keep Me", Answers = new List<Answer>() },
                new LearningItem { Id = itemIdToRemove, QuestionContent = "Remove Me", Answers = new List<Answer>() }
            }
        };

        _repositoryMock.Setup(r => r.GetWithItemsForUpdateAsync(setId))
            .ReturnsAsync(existingSet);
        _repositoryMock.Setup(r => r.GetWithItemsAsync(setId))
            .ReturnsAsync(existingSet);

        var updateDto = new UpdateLearningSetDto(
            "Test Set",
            null,
            "Quiz",
            Guid.NewGuid(), // CategoryId
            new List<UpdateLearningItemDto>
            {
                new UpdateLearningItemDto(itemIdToKeep, "Keep Me", new List<AnswerDto>(), null, null)
            }
        );

        // Act
        await _service.UpdateSetAsync(setId, updateDto, userId);

        // Assert
        existingSet.Items.Should().HaveCount(2); // Should NOT be removed from collection
        existingSet.Items.First(i => i.Id == itemIdToRemove).IsDeleted.Should().BeTrue();
        existingSet.Items.First(i => i.Id == itemIdToKeep).IsDeleted.Should().BeFalse();
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateSetAsync_Should_Not_Add_Item_With_Empty_Content()
    {
        // Arrange
        var setId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        var existingSet = new LearningSet 
        { 
            Id = setId, 
            Title = "Test Set", 
            Type = SetType.Quiz, 
            OwnerId = userId,
            Items = new List<LearningItem>()
        };

        _repositoryMock.Setup(r => r.GetWithItemsForUpdateAsync(setId))
            .ReturnsAsync(existingSet);
        _repositoryMock.Setup(r => r.GetWithItemsAsync(setId))
            .ReturnsAsync(existingSet);

        var updateDto = new UpdateLearningSetDto(
            "Test Set",
            null,
            "Quiz",
            Guid.NewGuid(),
            new List<UpdateLearningItemDto>
            {
                new UpdateLearningItemDto(null, "   ", new List<AnswerDto>(), null, null), // Empty question
                new UpdateLearningItemDto(null, "Valid Question", new List<AnswerDto> { new AnswerDto(null, "  ", false) }, null, null) // Question with empty answer
            }
        );

        // Act
        await _service.UpdateSetAsync(setId, updateDto, userId);

        // Assert
        existingSet.Items.Should().HaveCount(1);
        existingSet.Items.First().QuestionContent.Should().Be("Valid Question");
        existingSet.Items.First().Answers.Should().BeEmpty();
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
