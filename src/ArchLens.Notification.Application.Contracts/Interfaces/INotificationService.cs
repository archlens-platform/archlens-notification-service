namespace ArchLens.Notification.Application.Contracts.Interfaces;

public interface INotificationService
{
    Task NotifyStatusChangedAsync(Guid analysisId, string oldStatus, string newStatus, CancellationToken ct = default);
    Task NotifyAnalysisCompletedAsync(Guid analysisId, Guid diagramId, CancellationToken ct = default);
}
