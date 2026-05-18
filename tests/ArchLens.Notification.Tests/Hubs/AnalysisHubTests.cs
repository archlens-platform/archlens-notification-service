using ArchLens.Notification.Infrastructure.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;

namespace ArchLens.Notification.Tests.Hubs;

public class AnalysisHubTests
{
    private readonly AnalysisHub _hub;
    private readonly IGroupManager _groupManager;
    private readonly HubCallerContext _callerContext;

    public AnalysisHubTests()
    {
        _hub = new AnalysisHub();
        _groupManager = Substitute.For<IGroupManager>();
        _callerContext = Substitute.For<HubCallerContext>();
        _callerContext.ConnectionId.Returns("test-connection-id");

        _hub.Groups = _groupManager;
        _hub.Context = _callerContext;
    }

    [Fact]
    public async Task JoinAnalysisGroup_ShouldAddToGroup()
    {
        // Arrange
        var analysisId = Guid.NewGuid().ToString();

        // Act
        await _hub.JoinAnalysisGroup(analysisId);

        // Assert
        await _groupManager.Received(1).AddToGroupAsync(
            "test-connection-id",
            analysisId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LeaveAnalysisGroup_ShouldRemoveFromGroup()
    {
        // Arrange
        var analysisId = Guid.NewGuid().ToString();

        // Act
        await _hub.LeaveAnalysisGroup(analysisId);

        // Assert
        await _groupManager.Received(1).RemoveFromGroupAsync(
            "test-connection-id",
            analysisId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task JoinDashboard_ShouldAddToDashboardGroup()
    {
        // Act
        await _hub.JoinDashboard();

        // Assert
        await _groupManager.Received(1).AddToGroupAsync(
            "test-connection-id",
            "dashboard",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldAddToDashboardGroup()
    {
        // Arrange
        var hubClients = Substitute.For<IHubCallerClients>();
        _hub.Clients = hubClients;

        // Act
        await _hub.OnConnectedAsync();

        // Assert
        await _groupManager.Received(1).AddToGroupAsync(
            "test-connection-id",
            "dashboard",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task JoinAnalysisGroup_WithEmptyString_ShouldStillAddToGroup()
    {
        // Act
        await _hub.JoinAnalysisGroup(string.Empty);

        // Assert
        await _groupManager.Received(1).AddToGroupAsync(
            "test-connection-id",
            string.Empty,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LeaveAnalysisGroup_WithEmptyString_ShouldStillRemoveFromGroup()
    {
        // Act
        await _hub.LeaveAnalysisGroup(string.Empty);

        // Assert
        await _groupManager.Received(1).RemoveFromGroupAsync(
            "test-connection-id",
            string.Empty,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task JoinAnalysisGroup_ShouldUseCorrectConnectionId()
    {
        // Arrange
        var specificConnectionId = "specific-conn-123";
        _callerContext.ConnectionId.Returns(specificConnectionId);
        var analysisId = "analysis-456";

        // Act
        await _hub.JoinAnalysisGroup(analysisId);

        // Assert
        await _groupManager.Received(1).AddToGroupAsync(
            specificConnectionId,
            analysisId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LeaveAnalysisGroup_ShouldUseCorrectConnectionId()
    {
        // Arrange
        var specificConnectionId = "specific-conn-789";
        _callerContext.ConnectionId.Returns(specificConnectionId);
        var analysisId = "analysis-101";

        // Act
        await _hub.LeaveAnalysisGroup(analysisId);

        // Assert
        await _groupManager.Received(1).RemoveFromGroupAsync(
            specificConnectionId,
            analysisId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task JoinDashboard_ShouldUseCorrectConnectionId()
    {
        // Arrange
        var specificConnectionId = "dashboard-conn-555";
        _callerContext.ConnectionId.Returns(specificConnectionId);

        // Act
        await _hub.JoinDashboard();

        // Assert
        await _groupManager.Received(1).AddToGroupAsync(
            specificConnectionId,
            "dashboard",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldUseCorrectConnectionId()
    {
        // Arrange
        var specificConnectionId = "on-connect-conn-999";
        _callerContext.ConnectionId.Returns(specificConnectionId);
        var hubClients = Substitute.For<IHubCallerClients>();
        _hub.Clients = hubClients;

        // Act
        await _hub.OnConnectedAsync();

        // Assert
        await _groupManager.Received(1).AddToGroupAsync(
            specificConnectionId,
            "dashboard",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task JoinAnalysisGroup_WithGuidString_ShouldAddToGroup()
    {
        // Arrange
        var analysisId = Guid.NewGuid().ToString();

        // Act
        await _hub.JoinAnalysisGroup(analysisId);

        // Assert
        await _groupManager.Received(1).AddToGroupAsync(
            Arg.Any<string>(),
            analysisId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task JoinAndLeaveAnalysisGroup_ShouldCallBothMethods()
    {
        // Arrange
        var analysisId = "test-analysis";

        // Act
        await _hub.JoinAnalysisGroup(analysisId);
        await _hub.LeaveAnalysisGroup(analysisId);

        // Assert
        await _groupManager.Received(1).AddToGroupAsync(
            "test-connection-id",
            analysisId,
            Arg.Any<CancellationToken>());
        await _groupManager.Received(1).RemoveFromGroupAsync(
            "test-connection-id",
            analysisId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void AnalysisHub_ShouldInheritFromHub()
    {
        // Assert
        _hub.Should().BeAssignableTo<Hub>();
    }
}
