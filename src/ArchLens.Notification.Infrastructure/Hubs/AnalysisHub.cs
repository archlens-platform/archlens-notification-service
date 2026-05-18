using Microsoft.AspNetCore.SignalR;

namespace ArchLens.Notification.Infrastructure.Hubs;

public sealed class AnalysisHub : Hub
{
    public async Task JoinAnalysisGroup(string analysisId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, analysisId);
    }

    public async Task LeaveAnalysisGroup(string analysisId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, analysisId);
    }

    public async Task JoinDashboard()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "dashboard");
        await base.OnConnectedAsync();
    }
}
