using Microsoft.AspNetCore.SignalR;
using RealtimeService.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. Add SignalR Services
builder.Services.AddSignalR();

// 2. Configure CORS for SignalR (Strictly required for WebSockets to work across domains)
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRCorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // Allow frontend on any domain (Vercel, Localhost)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Crucial: SignalR requires credentials (cookies/auth headers) to establish a connection
    });
});

var app = builder.Build();

app.UseCors("SignalRCorsPolicy");

// 3. Map the SignalR Hub to a specific route
app.MapHub<PollHub>("/hubs/poll");


// This endpoint listens for POST requests from VoteService.
// When VoteService saves a vote, it sends the new totals here.
app.MapPost("/internal/broadcast/{code}", async (string code, List<OptionCountDto> results, IHubContext<PollHub> hubContext) =>
{
    // Broadcast the new results ONLY to clients in the specific poll group
    // The frontend will listen for the event named "ReceiveVoteUpdate"
    await hubContext.Clients.Group(code).SendAsync("ReceiveVoteUpdate", results);

    return Results.Ok(new { message = $"Data successfully broadcasted to group: {code}" });
});

app.Run();

// ==========================================
// DATA TRANSFER OBJECTS (DTOs)
// ==========================================
public record OptionCountDto(int OptionIndex, int Count);