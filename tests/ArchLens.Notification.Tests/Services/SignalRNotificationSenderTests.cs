using ArchLens.Notification.Domain.Interfaces.NotificationInterfaces;
using ArchLens.Notification.Infrastructure.Hubs;
using ArchLens.Notification.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace ArchLens.Notification.Tests.Services;

public class SignalRNotificationSenderTests
{
    private readonly IHubContext<AnalysisHub> _hubContext;
    private readonly IHubClients _hubClients;
    private readonly IClientProxy _analysisGroupProxy;
    private readonly IClientProxy _dashboardProxy;
    private readonly SignalRNotificationSender _sender;

    public SignalRNotificationSenderTests()
    {
        _hubContext = Substitute.For<IHubContext<AnalysisHub>>();
        _hubClients = Substitute.For<IHubClients>();
        _analysisGroupProxy = Substitute.For<IClientProxy>();
        _dashboardProxy = Substitute.For<IClientProxy>();

        _hubContext.Clients.Returns(_hubClients);
        _hubClients.Group("dashboard").Returns(_dashboardProxy);

        _sender = new SignalRNotificationSender(_hubContext);
    }

    [Fact]
    public async Task SendStatusChangedAsync_ShouldSendToAnalysisGroup()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        _hubClients.Group(analysisId.ToString()).Returns(_analysisGroupProxy);

        // Act
        await _sender.SendStatusChangedAsync(analysisId, "Pending", "Processing", DateTime.UtcNow);

        // Assert
        await _analysisGroupProxy.Received(1).SendCoreAsync(
            "StatusChanged",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendStatusChangedAsync_ShouldSendToDashboardGroup()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        _hubClients.Group(analysisId.ToString()).Returns(_analysisGroupProxy);

        // Act
        await _sender.SendStatusChangedAsync(analysisId, "Processing", "Completed", DateTime.UtcNow);

        // Assert
        await _dashboardProxy.Received(1).SendCoreAsync(
            "AnalysisStatusChanged",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAnalysisCompletedAsync_ShouldSendToAnalysisGroup()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        var diagramId = Guid.NewGuid();
        _hubClients.Group(analysisId.ToString()).Returns(_analysisGroupProxy);

        // Act
        await _sender.SendAnalysisCompletedAsync(analysisId, diagramId, DateTime.UtcNow);

        // Assert
        await _analysisGroupProxy.Received(1).SendCoreAsync(
            "AnalysisCompleted",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAnalysisCompletedAsync_ShouldSendToDashboardGroup()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        var diagramId = Guid.NewGuid();
        _hubClients.Group(analysisId.ToString()).Returns(_analysisGroupProxy);

        // Act
        await _sender.SendAnalysisCompletedAsync(analysisId, diagramId, DateTime.UtcNow);

        // Assert
        await _dashboardProxy.Received(1).SendCoreAsync(
            "AnalysisCompleted",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BroadcastAsync_ShouldSendToDashboardGroup()
    {
        // Arrange
        var eventName = "CustomEvent";
        var payload = new { Message = "test" };

        // Act
        await _sender.BroadcastAsync(eventName, payload);

        // Assert
        await _dashboardProxy.Received(1).SendCoreAsync(
            eventName,
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BroadcastAsync_WithCancellationToken_ShouldPassToken()
    {
        // Arrange
        var eventName = "TestEvent";
        var payload = new { Data = "value" };
        using var cts = new CancellationTokenSource();

        // Act
        await _sender.BroadcastAsync(eventName, payload, cts.Token);

        // Assert
        await _dashboardProxy.Received(1).SendCoreAsync(
            eventName,
            Arg.Any<object?[]>(),
            cts.Token);
    }

    [Fact]
    public async Task SendStatusChangedAsync_WithCancellationToken_ShouldPassToken()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        _hubClients.Group(analysisId.ToString()).Returns(_analysisGroupProxy);
        using var cts = new CancellationTokenSource();

        // Act
        await _sender.SendStatusChangedAsync(analysisId, "Old", "New", DateTime.UtcNow, cts.Token);

        // Assert
        await _analysisGroupProxy.Received(1).SendCoreAsync(
            "StatusChanged",
            Arg.Any<object?[]>(),
            cts.Token);
    }

    [Fact]
    public async Task SendAnalysisCompletedAsync_WithCancellationToken_ShouldPassToken()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        var diagramId = Guid.NewGuid();
        _hubClients.Group(analysisId.ToString()).Returns(_analysisGroupProxy);
        using var cts = new CancellationTokenSource();

        // Act
        await _sender.SendAnalysisCompletedAsync(analysisId, diagramId, DateTime.UtcNow, cts.Token);

        // Assert
        await _analysisGroupProxy.Received(1).SendCoreAsync(
            "AnalysisCompleted",
            Arg.Any<object?[]>(),
            cts.Token);
    }

    [Fact]
    public async Task SendStatusChangedAsync_ShouldUseCorrectGroupName()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        _hubClients.Group(analysisId.ToString()).Returns(_analysisGroupProxy);

        // Act
        await _sender.SendStatusChangedAsync(analysisId, "Pending", "Processing", DateTime.UtcNow);

        // Assert
        _hubClients.Received().Group(analysisId.ToString());
        _hubClients.Received().Group("dashboard");
    }

    [Fact]
    public async Task SendStatusChangedAsync_WithEmptyGuid_ShouldStillSend()
    {
        // Arrange
        var emptyProxy = Substitute.For<IClientProxy>();
        _hubClients.Group(Guid.Empty.ToString()).Returns(emptyProxy);

        // Act
        await _sender.SendStatusChangedAsync(Guid.Empty, "Old", "New", DateTime.UtcNow);

        // Assert
        await emptyProxy.Received(1).SendCoreAsync(
            "StatusChanged",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
        await _dashboardProxy.Received(1).SendCoreAsync(
            "AnalysisStatusChanged",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAnalysisCompletedAsync_WithEmptyGuids_ShouldStillSend()
    {
        // Arrange
        var emptyProxy = Substitute.For<IClientProxy>();
        _hubClients.Group(Guid.Empty.ToString()).Returns(emptyProxy);

        // Act
        await _sender.SendAnalysisCompletedAsync(Guid.Empty, Guid.Empty, DateTime.UtcNow);

        // Assert
        await emptyProxy.Received(1).SendCoreAsync(
            "AnalysisCompleted",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
        await _dashboardProxy.Received(1).SendCoreAsync(
            "AnalysisCompleted",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendStatusChangedAsync_WithEmptyStatuses_ShouldStillSend()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        _hubClients.Group(analysisId.ToString()).Returns(_analysisGroupProxy);

        // Act
        var act = () => _sender.SendStatusChangedAsync(analysisId, "", "", DateTime.MinValue);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task BroadcastAsync_WithEmptyEventName_ShouldStillSend()
    {
        // Arrange
        var payload = new { Info = "data" };

        // Act
        await _sender.BroadcastAsync("", payload);

        // Assert
        await _dashboardProxy.Received(1).SendCoreAsync(
            "",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAnalysisCompletedAsync_ShouldUseCorrectEventName()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        var diagramId = Guid.NewGuid();
        var analysisProxy = Substitute.For<IClientProxy>();
        var dashProxy = Substitute.For<IClientProxy>();
        _hubClients.Group(analysisId.ToString()).Returns(analysisProxy);
        _hubClients.Group("dashboard").Returns(dashProxy);

        // Act
        await _sender.SendAnalysisCompletedAsync(analysisId, diagramId, DateTime.UtcNow);

        // Assert - both groups receive "AnalysisCompleted" event name
        await analysisProxy.Received(1).SendCoreAsync(
            "AnalysisCompleted",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
        await dashProxy.Received(1).SendCoreAsync(
            "AnalysisCompleted",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAnalysisCompletedAsync_ShouldPassTokenToBothGroups()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        var diagramId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        var analysisProxy = Substitute.For<IClientProxy>();
        var dashProxy = Substitute.For<IClientProxy>();
        _hubClients.Group(analysisId.ToString()).Returns(analysisProxy);
        _hubClients.Group("dashboard").Returns(dashProxy);

        // Act
        await _sender.SendAnalysisCompletedAsync(analysisId, diagramId, DateTime.UtcNow, cts.Token);

        // Assert
        await analysisProxy.Received(1).SendCoreAsync(
            Arg.Any<string>(),
            Arg.Any<object?[]>(),
            cts.Token);
        await dashProxy.Received(1).SendCoreAsync(
            Arg.Any<string>(),
            Arg.Any<object?[]>(),
            cts.Token);
    }

    [Fact]
    public async Task SendStatusChangedAsync_ShouldPassTokenToBothGroups()
    {
        // Arrange
        var analysisId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        var analysisProxy = Substitute.For<IClientProxy>();
        var dashProxy = Substitute.For<IClientProxy>();
        _hubClients.Group(analysisId.ToString()).Returns(analysisProxy);
        _hubClients.Group("dashboard").Returns(dashProxy);

        // Act
        await _sender.SendStatusChangedAsync(analysisId, "A", "B", DateTime.UtcNow, cts.Token);

        // Assert
        await analysisProxy.Received(1).SendCoreAsync(
            Arg.Any<string>(),
            Arg.Any<object?[]>(),
            cts.Token);
        await dashProxy.Received(1).SendCoreAsync(
            Arg.Any<string>(),
            Arg.Any<object?[]>(),
            cts.Token);
    }

    [Fact]
    public void SignalRNotificationSender_ShouldImplementINotificationSender()
    {
        // Assert
        _sender.Should().BeAssignableTo<INotificationSender>();
    }

    [Fact]
    public async Task BroadcastAsync_WithComplexPayload_ShouldSend()
    {
        // Arrange
        var payload = new
        {
            Id = Guid.NewGuid(),
            Items = new[] { "a", "b", "c" },
            Nested = new { Value = 42 }
        };

        // Act
        await _sender.BroadcastAsync("ComplexEvent", payload);

        // Assert
        await _dashboardProxy.Received(1).SendCoreAsync(
            "ComplexEvent",
            Arg.Any<object?[]>(),
            Arg.Any<CancellationToken>());
    }
}
