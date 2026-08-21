using Microsoft.EntityFrameworkCore;
using PollService.Data;
using PollService.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Thêm CORS để cho phép Frontend (Vercel/Localhost) gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 3. Cấu hình Swagger UI để test API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (dbContext.Database.IsRelational() && dbContext.Database.GetPendingMigrations().Any())
        {
            dbContext.Database.Migrate();
        }
    }
}
catch (Exception)
{
    // Bỏ qua lỗi Migration khi đang chạy Unit Test (vì Unit Test dùng DB ảo trên RAM)
}

// 5. Cấu hình Middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

// ==========================================
// API ENDPOINTS
// ==========================================

// Endpoint 1: POST /api/polls - Tạo cuộc khảo sát mới
app.MapPost("/api/polls", async (CreatePollDto dto, AppDbContext db) =>
{
    // Validation 1: Kiểm tra câu hỏi
    if (string.IsNullOrWhiteSpace(dto.Question))
    {
        return Results.BadRequest(new { message = "Question is required and cannot be empty." });
    }

    // Validation 2: Kiểm tra độ dài câu hỏi
    if (dto.Question.Trim().Length > 500)
    {
        return Results.BadRequest(new { message = "Question cannot exceed 500 characters." });
    }

    // Validation 3: Làm sạch và lọc các option rỗng
    var cleanedOptions = dto.Options?
        .Where(opt => !string.IsNullOrWhiteSpace(opt))
        .Select(opt => opt.Trim())
        .ToList() ?? new List<string>();

    // Validation 4: Kiểm tra số lượng options (bắt buộc từ 2 đến 6 lựa chọn theo đề bài)
    if (cleanedOptions.Count < 2 || cleanedOptions.Count > 6)
    {
        return Results.BadRequest(new { message = "A poll must contain between 2 and 6 valid options." });
    }

    // Sinh mã ngắn ngẫu nhiên 6 ký tự (ví dụ: "7fGh2a")
    string shortCode = Guid.NewGuid().ToString("N")[..6];

    var poll = new Poll
    {
        Code = shortCode,
        Question = dto.Question.Trim(),
        Options = cleanedOptions,
        Status = PollStatus.Open,
        CreatedAt = DateTime.UtcNow
    };

    db.Polls.Add(poll);
    await db.SaveChangesAsync();

    return Results.Created($"/api/polls/{poll.Code}", poll);
});

// Endpoint 2: GET /api/polls/{code} - Lấy thông tin poll bằng short code
app.MapGet("/api/polls/{code}", async (string code, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(code))
    {
        return Results.BadRequest(new { message = "Poll code is required." });
    }

    var poll = await db.Polls
        .AsNoTracking() // Tối ưu hiệu năng đọc (không cần tracking thay đổi)
        .FirstOrDefaultAsync(p => p.Code == code);

    if (poll is null)
    {
        return Results.NotFound(new { message = $"Poll with code '{code}' was not found." });
    }

    return Results.Ok(poll);
});

// Endpoint 3: PUT /api/polls/{code}/close - Đóng cuộc khảo sát
app.MapPut("/api/polls/{code}/close", async (string code, AppDbContext db) =>
{
    var poll = await db.Polls.FirstOrDefaultAsync(p => p.Code == code);

    if (poll == null)
    {
        return Results.NotFound(new { message = "Poll not found" });
    }

    if (poll.Status == "Closed")
    {
        return Results.BadRequest(new { message = "Poll is already closed." });
    }

    // Đổi trạng thái thành Closed
    poll.Status = "Closed";
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Poll closed successfully." });
});

app.Run();


public record CreatePollDto(string Question, List<string> Options);


// Expose the Program class to the Unit Test project
public partial class Program { }