using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Tests.Unit;

public class ServiceBusMessageEntityTests
{
    [Fact]
    public void Create_WithValidArguments_ShouldInitialiseCorrectly()
    {
        // Act
        var message = ServiceBusMessage.Create(
            "msg-001", "Test Body", "OrderCreated", "corr-001");

        // Assert
        message.MessageId.Should().Be("msg-001");
        message.Body.Should().Be("Test Body");
        message.Subject.Should().Be("OrderCreated");
        message.CorrelationId.Should().Be("corr-001");
        message.Status.Should().Be(MessageStatus.Received);
        message.RetryCount.Should().Be(0);
        message.ProcessedAt.Should().BeNull();
        message.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("", "body", "subject")]
    [InlineData("id", "", "subject")]
    [InlineData("id", "body", "")]
    public void Create_WithInvalidArguments_ShouldThrow(
        string messageId, string body, string subject)
    {
        // Act
        var act = () => ServiceBusMessage.Create(messageId, body, subject);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MarkAsProcessed_ShouldUpdateStatusAndTimestamp()
    {
        // Arrange
        var message = ServiceBusMessage.Create("msg-001", "Body", "Subject");
        var before = DateTime.UtcNow;

        // Act
        message.MarkAsProcessed();

        // Assert
        message.Status.Should().Be(MessageStatus.Processed);
        message.ProcessedAt.Should().NotBeNull();
        message.ProcessedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void MarkAsFailed_ShouldIncrementRetryCount()
    {
        // Arrange
        var message = ServiceBusMessage.Create("msg-001", "Body", "Subject");

        // Act
        message.MarkAsFailed("Some error");

        // Assert
        message.Status.Should().Be(MessageStatus.Failed);
        message.RetryCount.Should().Be(1);
        message.ErrorMessage.Should().Be("Some error");
    }
}
