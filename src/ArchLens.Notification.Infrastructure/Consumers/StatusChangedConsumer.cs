using ArchLens.Contracts.Events;
using ArchLens.Notification.Infrastructure.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ArchLens.Notification.Infrastructure.Consumers;

public sealed class StatusChangedConsumer(
    IHubContext<AnalysisHub> hubContext,
    ILogger<StatusChangedConsumer> logger) : IConsumer<StatusChangedEvent>
{
    public async Task Consume(ConsumeContext<StatusChangedEvent> context)
    {
        var msg = context.Message;

        logger.LogInformation(
            "Status changed for analysis {AnalysisId}: {Old} -> {New}",
            msg.AnalysisId, msg.OldStatus, msg.NewStatus);

        var payload = new
        {
            msg.AnalysisId,
            msg.OldStatus,
            msg.NewStatus,
            msg.Timestamp
        };

        await hubContext.Clients.Group(msg.AnalysisId.ToString())
            .SendAsync("StatusChanged", payload, context.CancellationToken);

        await hubContext.Clients.Group("dashboard")
            .SendAsync("AnalysisStatusChanged", payload, context.CancellationToken);
    }
}
