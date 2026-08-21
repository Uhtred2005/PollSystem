using Microsoft.EntityFrameworkCore;
using VoteService.Data;
using VoteService.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cấu hình HttpClient để VoteService có thể gọi sang RealtimeService (Microservice Communication)
builder.Services.AddHttpClient("RealtimeClient", client =>
{
    // Lấy URL của RealtimeService từ appsettings, nếu không có thì mặc định gọi cổng 5003 (Local)
    client.BaseAddress = new Uri(builder.Configuration["RealtimeServiceUrl"] ?? "http://localhost:5003");
});

// 3. CORS & Swagger
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Tự động áp dụng Migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (dbContext.Database.GetPendingMigrations().Any())
    {
        dbContext.Database.Migrate();
    }
}

app.UseSwagger();
//app.UseSwaggerUI();
app.UseCors("AllowAll");

// Endpoint 1: POST /api/votes/{code} - Gửi phiếu bầu
app.MapPost("/api/votes/{code}", async (string code, VoteRequestDto dto, AppDbContext db, IHttpClientFactory clientFactory) =>
{
    if (string.IsNullOrWhiteSpace(dto.VoterToken))
    {
        return Results.BadRequest(new { message = "Voter token (Fingerprint) is missing." });
    }

    // 1. Kiểm tra trùng lặp (Anti-Cheat)
    bool alreadyVoted = await db.Votes.AnyAsync(v => v.PollCode == code && v.VoterToken == dto.VoterToken);
    if (alreadyVoted)
    {
        // Trả về mã 409 Conflict theo chuẩn RESTful khi có xung đột dữ liệu
        return Results.Conflict(new { message = "You have already voted in this poll." });
    }

    // 2. Lưu phiếu bầu
    var vote = new Vote
    {
        PollCode = code,
        OptionIndex = dto.OptionIndex,
        VoterToken = dto.VoterToken
    };

    db.Votes.Add(vote);
    await db.SaveChangesAsync();

    // 3. Tính toán lại tổng kết quả (Gom nhóm theo OptionIndex và đếm số lượng)
    var results = await db.Votes
        .Where(v => v.PollCode == code)
        .GroupBy(v => v.OptionIndex)
        .Select(g => new OptionCountDto(g.Key, g.Count()))
        .ToListAsync();

    // 4. Bắn tín hiệu sang RealtimeService để cập nhật biểu đồ
    // (Dùng Try-Catch để nếu RealtimeService có sập thì Vote vẫn thành công)
    try
    {
        var client = clientFactory.CreateClient("RealtimeClient");
        await client.PostAsJsonAsync($"/internal/broadcast/{code}", results);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Warning] Failed to broadcast realtime update: {ex.Message}");
    }

    return Results.Ok(new { message = "Vote cast successfully." });
});

// Endpoint 2: GET /api/votes/{code}/results - Lấy tổng số vote hiện tại
app.MapGet("/api/votes/{code}/results", async (string code, AppDbContext db) =>
{
    // Đếm số phiếu cho từng option
    var results = await db.Votes
        .Where(v => v.PollCode == code)
        .GroupBy(v => v.OptionIndex)
        .Select(g => new OptionCountDto(g.Key, g.Count()))
        .ToListAsync();

    return Results.Ok(results);
});

app.Run();

public record VoteRequestDto(int OptionIndex, string VoterToken);
public record OptionCountDto(int OptionIndex, int Count);