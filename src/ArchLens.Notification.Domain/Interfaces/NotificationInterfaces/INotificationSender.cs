namespace ArchLens.Notification.Domain.Interfaces.NotificationInterfaces;

public interface INotificationSender
{
    Task SendStatusChangedAsync(Guid analysisId, string oldStatus, string newStatus, DateTime timestamp, CancellationToken ct = default);
    Task SendAnalysisCompletedAsync(Guid analysisId, Guid diagramId, DateTime timestamp, CancellationToken ct = default);
    Task BroadcastAsync(string eventName, object payload, CancellationToken ct = default);
}
