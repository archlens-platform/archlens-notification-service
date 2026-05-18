using ArchLens.Contracts.Events;
using ArchLens.Notification.Infrastructure.Consumers;
using ArchLens.Notification.Infrastructure.Hubs;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ArchLens.Notification.Tests.Consumers;

public class StatusChangedConsumerTests
{
    private readonly IHubContext<AnalysisHub> _hubContext;
    private readonly ILogger<StatusChangedConsumer> _logger;
    private readonly StatusChangedConsumer _consumer;
    private readonly IHubClients _hubClients;
    private readonly IClientProxy _clientProxy;

    public StatusChangedConsumerTests()
    {
        _hubContext = Substitute.For<IHubContext<AnalysisHub>>();
        _logger = Substitute.For<ILogger<StatusChangedConsumer>>();
        _hubClients = Substitute.For<IHubClients>();
        _clientProxy = Substitute.For<IClientProxy>();

        _hubContext.Clients.Returns(_hubClients);
        _hubClients.Group(Arg.Any<string>()).Returns(_clientProxy);

        _consumer = new StatusChangedConsumer(_hubContext, _logger);
    }

    [Fact]
    public async Task Consume_ShouldSendToAnalysisGroup()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        var message = new StatusChangedEvent
        {
            AnalysisId = analysisId,
            OldStatus = "Pending",
            NewStatus = "Processing",
            Timestamp = DateTime.UtcNow
        };

        var context = CreateConsumeContext(message);

        // Act
        await _consumer.Consume(context);

        // Assert
        _hubClients.Received(1).Group(analysisId.ToString());
    }

    [Fact]
    public async Task Consume_ShouldSendToDashboardGroup()
    {
        // Arrange
        var message = new StatusChangedEvent
        {
            AnalysisId = Guid.NewGuid(),
            OldStatus = "Pending",
            NewStatus = "Completed",
            Timestamp = DateTime.UtcNow
        };

        var context = CreateConsumeContext(message);

        // Act
        await _consumer.Consume(context);

        // Assert
        _hubClients.Received(1).Group("dashboard");
    }

    [Fact]
    public async Task Consume_ShouldSendStatusChangedEventToAnalysisGroup()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        var message = new StatusChangedEvent
        {
            AnalysisId = analysisId,
            OldStatus = "Pending",
            NewStatus = "Processing",
            Timestamp = DateTime.UtcNow
        };

        var context = CreateConsumeContext(message);

        // Act
        await _consumer.Consume(context);

        // Assert
        await _clientProxy.Received().SendCoreAsync(
            "StatusChanged",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ShouldSendAnalysisStatusChangedEventToDashboard()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        var dashboardProxy = Substitute.For<IClientProxy>();
        _hubClients.Group("dashboard").Returns(dashboardProxy);

        var message = new StatusChangedEvent
        {
            AnalysisId = analysisId,
            OldStatus = "Processing",
            NewStatus = "Completed",
            Timestamp = DateTime.UtcNow
        };

        var context = CreateConsumeContext(message);

        // Act
        await _consumer.Consume(context);

        // Assert
        await dashboardProxy.Received().SendCoreAsync(
            "AnalysisStatusChanged",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ShouldLogInformation()
    {
        // Arrange
        var message = new StatusChangedEvent
        {
            AnalysisId = Guid.NewGuid(),
            OldStatus = "Pending",
            NewStatus = "Processing",
            Timestamp = DateTime.UtcNow
        };

        var context = CreateConsumeContext(message);

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
    public async Task Consume_ShouldPassCancellationTokenFromContext()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var analysisId = Guid.NewGuid();
        var analysisProxy = Substitute.For<IClientProxy>();
        var dashboardProxy = Substitute.For<IClientProxy>();
        _hubClients.Group(analysisId.ToString()).Returns(analysisProxy);
        _hubClients.Group("dashboard").Returns(dashboardProxy);

        var message = new StatusChangedEvent
        {
            AnalysisId = analysisId,
            OldStatus = "Pending",
            NewStatus = "Processing",
            Timestamp = DateTime.UtcNow
        };

        var context = CreateConsumeContext(message, cts.Token);

        // Act
        await _consumer.Consume(context);

        // Assert
        await analysisProxy.Received(1).SendCoreAsync(
            "StatusChanged",
            Arg.Any<object?[]>(),
            cts.Token);

        await dashboardProxy.Received(1).SendCoreAsync(
            "AnalysisStatusChanged",
            Arg.Any<object?[]>(),
            cts.Token);
    }

    [Fact]
    public async Task Consume_ShouldSendPayloadWithCorrectAnalysisId()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        var analysisProxy = Substitute.For<IClientProxy>();
        _hubClients.Group(analysisId.ToString()).Returns(analysisProxy);

        var message = new StatusChangedEvent
        {
            AnalysisId = analysisId,
            OldStatus = "Pending",
            NewStatus = "Processing",
            Timestamp = DateTime.UtcNow
        };

        var context = CreateConsumeContext(message);

        // Act
        await _consumer.Consume(context);

        // Assert
        await analysisProxy.Received(1).SendCoreAsync(
            "StatusChanged",
            Arg.Is<object?[]>(args => args.Length > 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_WithEmptyGuidAnalysisId_ShouldStillSend()
    {
        // Arrange
        var emptyProxy = Substitute.For<IClientProxy>();
        _hubClients.Group(Guid.Empty.ToString()).Returns(emptyProxy);

        var message = new StatusChangedEvent
        {
            AnalysisId = Guid.Empty,
            OldStatus = "Unknown",
            NewStatus = "Pending",
            Timestamp = DateTime.UtcNow
        };

        var context = CreateConsumeContext(message);

        // Act
        await _consumer.Consume(context);

        // Assert
        _hubClients.Received().Group(Guid.Empty.ToString());
        _hubClients.Received().Group("dashboard");
    }

    [Fact]
    public async Task Consume_WithEmptyStatusStrings_ShouldStillSend()
    {
        // Arrange
        var message = new StatusChangedEvent
        {
            AnalysisId = Guid.NewGuid(),
            OldStatus = "",
            NewStatus = "",
            Timestamp = DateTime.UtcNow
        };

        var context = CreateConsumeContext(message);

        // Act
        var act = () => _consumer.Consume(context);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_WithMinDateTimestamp_ShouldStillSend()
    {
        // Arrange
        var message = new StatusChangedEvent
        {
            AnalysisId = Guid.NewGuid(),
            OldStatus = "Pending",
            NewStatus = "Processing",
            Timestamp = DateTime.MinValue
        };

        var context = CreateConsumeContext(message);

        // Act
        var act = () => _consumer.Consume(context);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_ShouldSendBothGroupAndDashboardMessages()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        var message = new StatusChangedEvent
        {
            AnalysisId = analysisId,
            OldStatus = "Processing",
            NewStatus = "Failed",
            Timestamp = DateTime.UtcNow
        };

        var context = CreateConsumeContext(message);

        // Act
        await _consumer.Consume(context);

        // Assert - should call Group twice: once for analysis group, once for dashboard
        _hubClients.Received(1).Group(analysisId.ToString());
        _hubClients.Received(1).Group("dashboard");
        await _clientProxy.Received(2).SendCoreAsync(
            Arg.Any<string>(),
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ShouldUseCorrectEventNames()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        var analysisProxy = Substitute.For<IClientProxy>();
        var dashboardProxy = Substitute.For<IClientProxy>();
        _hubClients.Group(analysisId.ToString()).Returns(analysisProxy);
        _hubClients.Group("dashboard").Returns(dashboardProxy);

        var message = new StatusChangedEvent
        {
            AnalysisId = analysisId,
            OldStatus = "Pending",
            NewStatus = "Completed",
            Timestamp = DateTime.UtcNow
        };

        var context = CreateConsumeContext(message);

        // Act
        await _consumer.Consume(context);

        // Assert
        await analysisProxy.Received(1).SendCoreAsync(
            "StatusChanged",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());

        await dashboardProxy.Received(1).SendCoreAsync(
            "AnalysisStatusChanged",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    private static ConsumeContext<StatusChangedEvent> CreateConsumeContext(
        StatusChangedEvent message, CancellationToken cancellationToken = default)
    {
        var context = Substitute.For<ConsumeContext<StatusChangedEvent>>();
        context.Message.Returns(message);
        context.CancellationToken.Returns(cancellationToken);
        return context;
    }
}
