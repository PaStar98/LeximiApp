using Leximi.Domain.Entities;
using Xunit;
using FluentAssertions;

namespace Leximi.Domain.UnitTests.Entities;

public class LearningSetTests
{
    [Fact]
    public void LearningSet_Should_Initialize_With_Empty_Items_List()
    {
        // Arrange & Act
        var learningSet = new LearningSet
        {
            Title = "Test Set",
            Type = Domain.Enums.SetType.Flashcards
        };

        // Assert
        learningSet.Items.Should().NotBeNull();
        learningSet.Items.Should().BeEmpty();
    }

    [Fact]
    public void LearningItem_Should_Link_To_LearningSet()
    {
        // Arrange
        var setId = Guid.NewGuid();
        var learningSet = new LearningSet
        {
            Id = setId,
            Title = "Math Basics",
            Type = Domain.Enums.SetType.Flashcards
        };

        var item = new LearningItem
        {
            LearningSetId = setId,
            LearningSet = learningSet
        };

        // Act
        learningSet.Items.Add(item);

        // Assert
        learningSet.Items.Should().Contain(item);
        item.LearningSetId.Should().Be(setId);
    }
}
