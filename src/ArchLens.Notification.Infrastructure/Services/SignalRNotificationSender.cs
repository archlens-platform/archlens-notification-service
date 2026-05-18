using ArchLens.Notification.Domain.Interfaces.NotificationInterfaces;
using ArchLens.Notification.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ArchLens.Notification.Infrastructure.Services;

public sealed class SignalRNotificationSender(IHubContext<AnalysisHub> hubContext) : INotificationSender
{
    public async Task SendStatusChangedAsync(
        Guid analysisId, string oldStatus, string newStatus, DateTime timestamp, CancellationToken ct = default)
    {
        var payload = new { AnalysisId = analysisId, OldStatus = oldStatus, NewStatus = newStatus, Timestamp = timestamp };

        await hubContext.Clients.Group(analysisId.ToString())
            .SendAsync("StatusChanged", payload, ct);

        await hubContext.Clients.Group("dashboard")
            .SendAsync("AnalysisStatusChanged", payload, ct);
    }

    public async Task SendAnalysisCompletedAsync(
        Guid analysisId, Guid diagramId, DateTime timestamp, CancellationToken ct = default)
    {
        var payload = new { AnalysisId = analysisId, DiagramId = diagramId, Timestamp = timestamp };

        await hubContext.Clients.Group(analysisId.ToString())
            .SendAsync("AnalysisCompleted", payload, ct);

        await hubContext.Clients.Group("dashboard")
            .SendAsync("AnalysisCompleted", payload, ct);
    }

    public async Task BroadcastAsync(string eventName, object payload, CancellationToken ct = default)
    {
        await hubContext.Clients.Group("dashboard")
            .SendAsync(eventName, payload, ct);
    }
}
