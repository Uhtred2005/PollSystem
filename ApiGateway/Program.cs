var builder = WebApplication.CreateBuilder(args);

// 1. Configure CORS - Allow Frontend to call the Gateway
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // Accept requests from any origin (e.g., localhost:3000, Vercel)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR WebSocket connection
    });
});

// 2. Add YARP Reverse Proxy and load config from appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Apply CORS policy
app.UseCors();

// 3. Map the proxy routes
app.MapReverseProxy();

app.Run();