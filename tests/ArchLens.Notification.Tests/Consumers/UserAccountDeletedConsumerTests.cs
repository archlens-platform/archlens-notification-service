using ArchLens.Contracts.Events;
using ArchLens.Notification.Infrastructure.Consumers;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ArchLens.Notification.Tests.Consumers;

public class UserAccountDeletedConsumerTests
{
    private readonly ILogger<UserAccountDeletedConsumer> _logger;
    private readonly UserAccountDeletedConsumer _consumer;

    public UserAccountDeletedConsumerTests()
    {
        _logger = Substitute.For<ILogger<UserAccountDeletedConsumer>>();
        _consumer = new UserAccountDeletedConsumer(_logger);
    }

    [Fact]
    public async Task Consume_ShouldLogInformation()
    {
        // Arrange
        var message = new UserAccountDeletedEvent
        {
            UserId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow
        };

        var context = Substitute.For<ConsumeContext<UserAccountDeletedEvent>>();
        context.Message.Returns(message);

        // Act
        await _consumer.Consume(context);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Consume_ShouldCompleteSuccessfully()
    {
        // Arrange
        var message = new UserAccountDeletedEvent
        {
            UserId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow
        };

        var context = Substitute.For<ConsumeContext<UserAccountDeletedEvent>>();
        context.Message.Returns(message);

        // Act
        var act = () => _consumer.Consume(context);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_WithEmptyUserId_ShouldStillComplete()
    {
        // Arrange
        var message = new UserAccountDeletedEvent
        {
            UserId = Guid.Empty,
            Timestamp = DateTime.UtcNow
        };

        var context = Substitute.For<ConsumeContext<UserAccountDeletedEvent>>();
        context.Message.Returns(message);

        // Act
        var act = () => _consumer.Consume(context);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_ShouldReturnCompletedTask()
    {
        // Arrange
        var message = new UserAccountDeletedEvent
        {
            UserId = Guid.NewGuid(),
            Timestamp = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        var context = Substitute.For<ConsumeContext<UserAccountDeletedEvent>>();
        context.Message.Returns(message);

        // Act
        var task = _consumer.Consume(context);

        // Assert
        task.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task Consume_ShouldAccessMessageUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var message = new UserAccountDeletedEvent
        {
            UserId = userId,
            Timestamp = DateTime.UtcNow
        };

        var context = Substitute.For<ConsumeContext<UserAccountDeletedEvent>>();
        context.Message.Returns(message);

        // Act
        await _consumer.Consume(context);

        // Assert - verify the context.Message was accessed
        _ = context.Received().Message;
    }

    [Fact]
    public async Task Consume_WithMinDateTimestamp_ShouldStillComplete()
    {
        // Arrange
        var message = new UserAccountDeletedEvent
        {
            UserId = Guid.NewGuid(),
            Timestamp = DateTime.MinValue
        };

        var context = Substitute.For<ConsumeContext<UserAccountDeletedEvent>>();
        context.Message.Returns(message);

        // Act
        var act = () => _consumer.Consume(context);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_WithMaxDateTimestamp_ShouldStillComplete()
    {
        // Arrange
        var message = new UserAccountDeletedEvent
        {
            UserId = Guid.NewGuid(),
            Timestamp = DateTime.MaxValue
        };

        var context = Substitute.For<ConsumeContext<UserAccountDeletedEvent>>();
        context.Message.Returns(message);

        // Act
        var act = () => _consumer.Consume(context);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_ShouldLogWithCorrectLogLevel()
    {
        // Arrange
        var message = new UserAccountDeletedEvent
        {
            UserId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow
        };

        var context = Substitute.For<ConsumeContext<UserAccountDeletedEvent>>();
        context.Message.Returns(message);

        // Act
        await _consumer.Consume(context);

        // Assert - should log at Information level, not Error or Warning
        _logger.DidNotReceive().Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());

        _logger.DidNotReceive().Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task Consume_MultipleInvocations_ShouldAllSucceed()
    {
        // Arrange
        var context1 = Substitute.For<ConsumeContext<UserAccountDeletedEvent>>();
        context1.Message.Returns(new UserAccountDeletedEvent
        {
            UserId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow
        });

        var context2 = Substitute.For<ConsumeContext<UserAccountDeletedEvent>>();
        context2.Message.Returns(new UserAccountDeletedEvent
        {
            UserId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow
        });

        // Act
        await _consumer.Consume(context1);
        await _consumer.Consume(context2);

        // Assert
        _logger.Received(2).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
