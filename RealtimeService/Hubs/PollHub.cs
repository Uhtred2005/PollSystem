using Microsoft.AspNetCore.SignalR;

namespace RealtimeService.Hubs;

public class PollHub : Hub
{
    // Called by the Frontend (React/Vue) when a user opens a poll result page
    public async Task JoinPollGroup(string pollCode)
    {
        // Add the current user's connection to a specific group named after the pollCode
        await Groups.AddToGroupAsync(Context.ConnectionId, pollCode);
    }

    // Called by the Frontend when a user leaves the result page
    public async Task LeavePollGroup(string pollCode)
    {
        // Remove the user from the group so they stop receiving updates for this poll
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, pollCode);
    }
}